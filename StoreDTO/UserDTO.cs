namespace StoreDTO
{
    public class UserDTO
    {
        public int UserId { get; set; }
        public string Login { get; set; } = string.Empty;

        public byte[] PasswordHash { get; set; } = [];
        public Guid PasswordSalt { get; set; }

        public int RoleId { get; set; }
        
        public string RoleName { get; set; } = string.Empty;

        public bool IsActive { get; set; }
        public DateTime RowInsertTime { get; set; }
    }
}