using StoreBL.Exceptions;
using StoreBL.Users;
using StoreDAL.EF.Interfaces;
using StoreDTO;

namespace StoreBL.Categories
{
    public class CategoryService : ICategoryService
    {
        public const int NameMaxLength = 50;
        public const int DescriptionMaxLength = 500;

        private readonly ICategoryDAL _categoryDal;
        private readonly IProductDAL _productDal;
        private readonly IUserSession _userSession;

        public CategoryService(ICategoryDAL categoryDal, IProductDAL productDal, IUserSession userSession)
        {
            _categoryDal = categoryDal;
            _productDal = productDal;
            _userSession = userSession;
        }

        public List<CategoryDTO> GetAll()
        {
            EnsureProductManager();
            return _categoryDal.GetAll()
                .OrderBy(c => c.CategoryName, StringComparer.CurrentCultureIgnoreCase)
                .ToList();
        }

        public CategoryDTO? GetById(int categoryId)
        {
            EnsureProductManager();
            return _categoryDal.GetById(categoryId);
        }

        public CategoryDTO Create(CategoryDTO category)
        {
            EnsureProductManager();

            var normalized = Normalize(category);
            normalized.CategoryId = 0;
            normalized.IsDeleted = false;     
            Validate(normalized);

            return _categoryDal.Create(normalized);
        }

        public bool Update(CategoryDTO category)
        {
            EnsureProductManager();

            var existing = _categoryDal.GetById(category.CategoryId);
            if (existing == null) return false;

            var normalized = Normalize(category);
            normalized.IsDeleted = existing.IsDeleted;
            Validate(normalized);

            return _categoryDal.Update(normalized);
        }

        public bool Delete(int categoryId)
        {
            EnsureProductManager();

            var category = _categoryDal.GetById(categoryId);
            if (category == null) return false;

            var productsCount = _productDal.GetAll().Count(p => p.CategoryId == categoryId);
            if (productsCount > 0)
            {
                throw new BusinessRuleException(
                    $"У категорії є товари ({productsCount}). Видалити можна лише порожню категорію — цю позначте на видалення.");
            }

            return _categoryDal.Delete(categoryId);
        }

        public bool MarkDeleted(int categoryId)
        {
            EnsureProductManager();

            var category = _categoryDal.GetById(categoryId);
            if (category == null) return false;

            category.IsDeleted = true;
            _categoryDal.Update(category);

            // усі товари видаленої категорії блокуються
            var products = _productDal.GetAll()
                .Where(p => p.CategoryId == categoryId && !p.IsBlocked);

            foreach (var product in products)
            {
                product.IsBlocked = true;
                _productDal.Update(product);
            }

            return true;
        }

        public bool Restore(int categoryId)
        {
            EnsureProductManager();

            var category = _categoryDal.GetById(categoryId);
            if (category == null) return false;

            category.IsDeleted = false;
            return _categoryDal.Update(category); // Товари не розблоковуються(вручну)

        }


        private void EnsureProductManager()
        {
            if (!_userSession.IsProductManager)
            {
                throw new BusinessRuleException("Доступ дозволено лише менеджеру товарів.");
            }
        }

        private static CategoryDTO Normalize(CategoryDTO category)
        {
            var description = category.Description?.Trim();
            return new CategoryDTO
            {
                CategoryId = category.CategoryId,
                CategoryName = (category.CategoryName ?? string.Empty).Trim(),
                Description = string.IsNullOrEmpty(description) ? null : description,
                IsDeleted = category.IsDeleted,
                RowInsertTime = category.RowInsertTime
            };
        }

        private void Validate(CategoryDTO category)
        {
            var errors = new List<string>();

            if (category.CategoryName.Length == 0)
                errors.Add("Назва категорії обов'язкова.");
            else if (category.CategoryName.Length > NameMaxLength)
                errors.Add($"Назва категорії не може бути довшою за {NameMaxLength} символів.");

            if (category.Description?.Length > DescriptionMaxLength)
                errors.Add($"Опис не може бути довшим за {DescriptionMaxLength} символів.");

            if (errors.Count == 0 && IsDuplicate(category))
                errors.Add($"Категорія \"{category.CategoryName}\" вже існує.");

            if (errors.Count > 0)
                throw new BusinessRuleException(errors);
        }

        private bool IsDuplicate(CategoryDTO category)
        {
            return _categoryDal.GetAll().Any(c =>
                c.CategoryId != category.CategoryId &&
                string.Equals(c.CategoryName, category.CategoryName, StringComparison.CurrentCultureIgnoreCase));
        }
    }
}