using System.Collections.Generic;

namespace StoreDAL.EF.Entities;

public partial class tblCategory
{
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public bool IsDeleted { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<tblProduct> Products { get; set; } = new List<tblProduct>();
}