using System;

namespace StoreDAL.EF.Entities;

public partial class tblPriceHistory
{
    public int PriceHistoryId { get; set; }

    public int ProductId { get; set; }

    public decimal OldPurchasePrice { get; set; }

    public decimal NewPurchasePrice { get; set; }

    public decimal OldSellingPrice { get; set; }

    public decimal NewSellingPrice { get; set; }

    public DateTime RowInsertTime { get; set; }

    public virtual tblProduct Product { get; set; } = null!;
}