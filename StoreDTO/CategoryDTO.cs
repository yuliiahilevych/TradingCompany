namespace StoreDTO
{
    public class CategoryDTO
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public bool IsDeleted { get; set; }
        public string? Description { get; set; }

    }
}