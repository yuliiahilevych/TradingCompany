namespace StoreDTO
{
    public class ProductDTO
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal PurchasePrice { get; set; }
        public decimal SellingPrice { get; set; }
        public int CategoryId { get; set; }
        public bool IsBlocked { get; set; }
        public DateTime RowInsertTime { get; set; }
        public DateTime? RowUpdateTime { get; set; }
    }
}