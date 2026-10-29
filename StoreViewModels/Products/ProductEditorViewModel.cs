using System.Globalization;
using StoreBL.Products;
using StoreDTO;
using StoreViewModels.Base;

namespace StoreViewModels.Products
{
    public class ProductEditorViewModel : ValidatableViewModelBase
    {
        private string _productName;
        private int? _categoryId;
        private string _purchasePriceText;
        private string _sellingPriceText;
        private bool _isDirty;

        public ProductEditorViewModel(ProductDTO product)
        {
            ProductId = product.ProductId;
            IsBlocked = product.IsBlocked;
            _productName = product.ProductName;
            _categoryId = product.CategoryId == 0 ? null : product.CategoryId;

            _purchasePriceText = IsNew ? string.Empty : product.PurchasePrice.ToString("0.00", CultureInfo.InvariantCulture);
            _sellingPriceText = IsNew ? string.Empty : product.SellingPrice.ToString("0.00", CultureInfo.InvariantCulture);
        }

        public int ProductId { get; }
        public bool IsBlocked { get; }
        public bool IsNew => ProductId == 0;
        public string Header => IsNew ? "Новий товар" : "Редагування товару";

        public string ProductName
        {
            get => _productName;
            set { if (SetProperty(ref _productName, value)) { IsDirty = true; ValidateProductName(); } }
        }

        public int? CategoryId
        {
            get => _categoryId;
            set { if (SetProperty(ref _categoryId, value)) { IsDirty = true; ValidateCategory(); } }
        }


        public string PurchasePriceText
        {
            get => _purchasePriceText;
            set { if (SetProperty(ref _purchasePriceText, value)) { IsDirty = true; ValidatePurchasePrice(); } }
        }

        public string SellingPriceText
        {
            get => _sellingPriceText;
            set { if (SetProperty(ref _sellingPriceText, value)) { IsDirty = true; ValidateSellingPrice(); } }
        }

        public bool IsDirty
        {
            get => _isDirty;
            private set => SetProperty(ref _isDirty, value);
        }

        public ProductDTO ToDto()
        {
            TryParsePrice(PurchasePriceText, out var purchase);
            TryParsePrice(SellingPriceText, out var selling);

            return new ProductDTO
            {
                ProductId = ProductId,
                ProductName = ProductName,
                CategoryId = CategoryId ?? 0,
                PurchasePrice = purchase,
                SellingPrice = selling,
                IsBlocked = IsBlocked
            };
        }

        protected override void ValidateAllProperties()
        {
            ValidateProductName();
            ValidateCategory();
            ValidatePurchasePrice();
            ValidateSellingPrice();
        }

        private void ValidateProductName()
        {
            var errors = new List<string>();
            var name = ProductName.Trim();

            if (name.Length == 0)
                errors.Add("Назва обов'язкова.");
            else if (name.Length > ProductService.NameMaxLength)
                errors.Add($"Не довше за {ProductService.NameMaxLength} символів.");

            SetErrors(nameof(ProductName), errors);
        }

        private void ValidateCategory()
        {
            var errors = new List<string>();
            if (CategoryId == null)
                errors.Add("Виберіть категорію.");

            SetErrors(nameof(CategoryId), errors);
        }

        private void ValidatePurchasePrice() =>
            SetErrors(nameof(PurchasePriceText), PriceErrors(PurchasePriceText));

        private void ValidateSellingPrice() =>
            SetErrors(nameof(SellingPriceText), PriceErrors(SellingPriceText));

        private static List<string> PriceErrors(string text)
        {
            var errors = new List<string>();

            if (text.Trim().Length == 0)
                errors.Add("Вкажіть ціну.");
            else if (!TryParsePrice(text, out var price))
                errors.Add("Ціна має бути числом, наприклад 199.99");
            else if (price < 0 || price > ProductService.MaxPrice)
                errors.Add($"Ціна має бути в межах 0–{ProductService.MaxPrice:0.00}.");

            return errors;
        }

        private static bool TryParsePrice(string text, out decimal price)
        {
            var normalized = text.Trim().Replace(',', '.');
            return decimal.TryParse(normalized, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign,
                CultureInfo.InvariantCulture, out price);
        }
    }
}