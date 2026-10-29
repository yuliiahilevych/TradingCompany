using StoreBL.Categories;
using StoreDTO;
using StoreViewModels.Base;

namespace StoreViewModels.Categories
{
    // Форма редагування однієї категорії - працює з копією даних
    public class CategoryEditorViewModel : ValidatableViewModelBase
    {
        private string _categoryName;
        private string _description;
        private bool _isDirty;

        public CategoryEditorViewModel(CategoryDTO category)
        {
            CategoryId = category.CategoryId;
            IsDeleted = category.IsDeleted;
            _categoryName = category.CategoryName;
            _description = category.Description ?? string.Empty;
        }

        public int CategoryId { get; }
        public bool IsDeleted { get; }
        public bool IsNew => CategoryId == 0;
        public string Header => IsNew ? "Нова категорія" : "Редагування категорії";

        public string CategoryName
        {
            get => _categoryName;
            set
            {
                if (SetProperty(ref _categoryName, value))
                {
                    IsDirty = true;
                    ValidateCategoryName();
                }
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                if (SetProperty(ref _description, value))
                {
                    IsDirty = true;
                    ValidateDescription();
                }
            }
        }

        public bool IsDirty //чи є незбережені зміни
        {
            get => _isDirty;
            private set => SetProperty(ref _isDirty, value);
        }

        public CategoryDTO ToDto() => new()
        {
            CategoryId = CategoryId,
            CategoryName = CategoryName,
            Description = Description,
            IsDeleted = IsDeleted
        };

        protected override void ValidateAllProperties()
        {
            ValidateCategoryName();
            ValidateDescription();
        }

        private void ValidateCategoryName()
        {
            var errors = new List<string>();
            var name = CategoryName.Trim();

            if (name.Length == 0)
                errors.Add("Назва обов'язкова.");
            else if (name.Length > CategoryService.NameMaxLength)
                errors.Add($"Не довше за {CategoryService.NameMaxLength} символів.");

            SetErrors(nameof(CategoryName), errors);
        }

        private void ValidateDescription()
        {
            var errors = new List<string>();

            if (Description.Trim().Length > CategoryService.DescriptionMaxLength)
                errors.Add($"Не довше за {CategoryService.DescriptionMaxLength} символів.");

            SetErrors(nameof(Description), errors);
        }
    }
}