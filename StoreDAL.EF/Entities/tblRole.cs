using System;
using System.Collections.Generic;

namespace StoreDAL.EF.Entities;

public partial class tblRole
{
    public int RoleId { get; set; }

    public string RoleName { get; set; } = null!;

    public DateTime RowInsertTime { get; set; }

    public virtual ICollection<tblUser> Users { get; set; } = new List<tblUser>();
}