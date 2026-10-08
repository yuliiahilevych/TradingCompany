using System.Collections.Generic;
using System;


namespace StoreDAL.EF.Entities;

public partial class tblProduct
{
    public int ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public decimal PurchasePrice { get; set; }

    public decimal SellingPrice { get; set; }

    public int CategoryId { get; set; }

    public bool IsBlocked { get; set; }

    public DateTime RowInsertTime { get; set; }

    public DateTime? RowUpdateTime { get; set; }

    public virtual tblCategory Category { get; set; } = null!;

    public virtual ICollection<tblPriceHistory> PriceHistories { get; set; } = new List<tblPriceHistory>();
}