using System;

namespace StoreDTO
{
    public class PriceHistoryDTO
    {
        public int PriceHistoryId { get; set; }
        public int ProductId { get; set; }
        public decimal OldPurchasePrice { get; set; }
        public decimal NewPurchasePrice { get; set; }
        public decimal OldSellingPrice { get; set; }
        public decimal NewSellingPrice { get; set; }
        public DateTime ChangedAt { get; set; }
    }
}