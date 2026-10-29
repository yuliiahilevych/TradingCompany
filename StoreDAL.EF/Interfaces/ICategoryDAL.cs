using System.Collections.Generic;
using StoreDTO;

namespace StoreDAL.EF.Interfaces
{
    public interface ICategoryDAL
    {
        CategoryDTO Create(CategoryDTO category);
        List<CategoryDTO> GetAll();
        CategoryDTO? GetById(int id);
        bool Update(CategoryDTO category);
        bool Delete(int id);
    }
}