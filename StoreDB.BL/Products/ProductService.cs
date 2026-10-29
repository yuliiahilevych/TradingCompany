using StoreBL.Exceptions;
using StoreBL.Users;
using StoreDAL.EF.Interfaces;
using StoreDTO;

namespace StoreBL.Products
{
    public class ProductService : IProductService
    {
        public const int NameMaxLength = 50;
        public const decimal MaxPrice = 99_999_999.99m;   

        private readonly IProductDAL _productDal;
        private readonly ICategoryDAL _categoryDal;
        private readonly IPriceHistoryDAL _priceHistoryDal;
        private readonly IUserSession _userSession;

        public ProductService(IProductDAL productDal, ICategoryDAL categoryDal,
                              IPriceHistoryDAL priceHistoryDal, IUserSession userSession)
        {
            _productDal = productDal;
            _categoryDal = categoryDal;
            _priceHistoryDal = priceHistoryDal;
            _userSession = userSession;
        }

        public List<ProductDTO> GetAll()
        {
            return Search(null, null);
        }

        public ProductDTO? GetById(int productId)
        {
            EnsureProductManager();
            return _productDal.GetById(productId);
        }

        public List<ProductDTO> Search(string? text, int? categoryId)
        {
            EnsureProductManager();

            var query = _productDal.GetAll().AsEnumerable();

            if (categoryId != null)
                query = query.Where(p => p.CategoryId == categoryId);

            var search = text?.Trim();
            if (!string.IsNullOrEmpty(search))
                query = query.Where(p => p.ProductName.Contains(search, StringComparison.CurrentCultureIgnoreCase));

            return query
                .OrderBy(p => p.ProductName, StringComparer.CurrentCultureIgnoreCase)
                .ToList();
        }

        public ProductDTO Create(ProductDTO product)
        {
            EnsureProductManager();

            var normalized = Normalize(product);
            normalized.ProductId = 0;
            normalized.IsBlocked = false;        
            Validate(normalized);

            return _productDal.Create(normalized);
        }

        public bool Update(ProductDTO product)
        {
            EnsureProductManager();

            var existing = _productDal.GetById(product.ProductId);
            if (existing == null) return false;

            var normalized = Normalize(product);
            normalized.IsBlocked = existing.IsBlocked;
            Validate(normalized);

            if (!_productDal.Update(normalized)) return false;

            if (existing.PurchasePrice != normalized.PurchasePrice ||
                existing.SellingPrice != normalized.SellingPrice)
            {
                _priceHistoryDal.Create(new PriceHistoryDTO
                {
                    ProductId = existing.ProductId,
                    OldPurchasePrice = existing.PurchasePrice,
                    NewPurchasePrice = normalized.PurchasePrice,
                    OldSellingPrice = existing.SellingPrice,
                    NewSellingPrice = normalized.SellingPrice
                });
            }

            return true;
        }

        public bool Delete(int productId)
        {
            EnsureProductManager();

            var product = _productDal.GetById(productId);
            if (product == null) return false;

            if (!product.IsBlocked)
            {
                throw new BusinessRuleException("Спочатку заблокуйте товар, а потім видаляйте.");
            }

            foreach (var record in _priceHistoryDal.GetAll().Where(h => h.ProductId == productId))
            {
                _priceHistoryDal.Delete(record.PriceHistoryId);
            }

            return _productDal.Delete(productId);
        }

        public bool Block(int productId)
        {
            EnsureProductManager();

            var product = _productDal.GetById(productId);
            if (product == null) return false;

            product.IsBlocked = true;
            return _productDal.Update(product);
        }

        public bool Unblock(int productId)
        {
            EnsureProductManager();

            var product = _productDal.GetById(productId);
            if (product == null) return false;

            var category = _categoryDal.GetById(product.CategoryId);
            if (category == null || category.IsDeleted)
            {
                throw new BusinessRuleException(
                    "Категорію товару позначено на видалення. Спочатку відновіть категорію.");
            }

            product.IsBlocked = false;
            return _productDal.Update(product);
        }

        public List<PriceHistoryDTO> GetPriceHistory(int productId)
        {
            EnsureProductManager();

            return _priceHistoryDal.GetAll()
                .Where(h => h.ProductId == productId)
                .OrderByDescending(h => h.RowInsertTime)
                .ToList();
        }

        private void EnsureProductManager()
        {
            if (!_userSession.IsProductManager)
            {
                throw new BusinessRuleException("Доступ дозволено лише менеджеру товарів.");
            }
        }

        private static ProductDTO Normalize(ProductDTO product)
        {
            return new ProductDTO
            {
                ProductId = product.ProductId,
                ProductName = (product.ProductName ?? string.Empty).Trim(),
                PurchasePrice = Math.Round(product.PurchasePrice, 2),
                SellingPrice = Math.Round(product.SellingPrice, 2),
                CategoryId = product.CategoryId,
                IsBlocked = product.IsBlocked,
                RowInsertTime = product.RowInsertTime,
                RowUpdateTime = product.RowUpdateTime
            };
        }

        private void Validate(ProductDTO product)
        {
            var errors = new List<string>();

            if (product.ProductName.Length == 0)
                errors.Add("Назва товару обов'язкова.");
            else if (product.ProductName.Length > NameMaxLength)
                errors.Add($"Назва товару не може бути довшою за {NameMaxLength} символів.");

            if (product.PurchasePrice < 0 || product.PurchasePrice > MaxPrice)
                errors.Add($"Вхідна ціна має бути в межах 0–{MaxPrice:0.00}.");

            if (product.SellingPrice < 0 || product.SellingPrice > MaxPrice)
                errors.Add($"Ціна продажу має бути в межах 0–{MaxPrice:0.00}.");

            var category = _categoryDal.GetById(product.CategoryId);
            if (category == null)
                errors.Add("Виберіть існуючу категорію.");
            else if (category.IsDeleted)
                errors.Add($"Категорію \"{category.CategoryName}\" позначено на видалення: додавати в неї товари не можна.");

            if (errors.Count == 0 && IsDuplicate(product))
                errors.Add($"Товар \"{product.ProductName}\" вже є в цій категорії.");

            if (errors.Count > 0)
                throw new BusinessRuleException(errors);
        }

        // Назва унікальна в межах категорії (у різних категоріях може повторюватися)
        private bool IsDuplicate(ProductDTO product)
        {
            return _productDal.GetAll().Any(p =>
                p.ProductId != product.ProductId &&
                p.CategoryId == product.CategoryId &&
                string.Equals(p.ProductName, product.ProductName, StringComparison.CurrentCultureIgnoreCase));
        }
    }
}