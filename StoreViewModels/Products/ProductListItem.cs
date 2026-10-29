using StoreDTO;

namespace StoreViewModels.Products
{
    public class ProductListItem
    {
        public ProductListItem(ProductDTO product, string categoryName)
        {
            Product = product;
            CategoryName = categoryName;
        }

        public ProductDTO Product { get; }

        public int ProductId => Product.ProductId;
        public string ProductName => Product.ProductName;
        public int CategoryId => Product.CategoryId;
        public string CategoryName { get; }
        public decimal PurchasePrice => Product.PurchasePrice;
        public decimal SellingPrice => Product.SellingPrice;
        public bool IsBlocked => Product.IsBlocked;
        public DateTime? RowUpdateTime => Product.RowUpdateTime;
    }
}