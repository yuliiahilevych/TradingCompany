using System;

namespace StoreDAL.EF.Entities;

public partial class tblUser
{
    public int UserId { get; set; }

    public string Login { get; set; } = null!;

    public byte[] PasswordHash { get; set; } = null!;

    public Guid PasswordSalt { get; set; }

    public int RoleId { get; set; }

    public bool IsActive { get; set; }

    public DateTime RowInsertTime { get; set; }

    public virtual tblRole Role { get; set; } = null!;
}

