using StoreDTO;

namespace StoreBL.Categories
{
    public interface ICategoryService
    {
        List<CategoryDTO> GetAll();
        CategoryDTO? GetById(int categoryId);

        CategoryDTO Create(CategoryDTO category);
        bool Update(CategoryDTO category);

        bool MarkDeleted(int categoryId); // Позначити на видалення: усі товари категорії блокуються
       
        bool Restore(int categoryId); // Зняти позначку- товари лишаються заблокованими 
        bool Delete(int categoryId);
    }
}