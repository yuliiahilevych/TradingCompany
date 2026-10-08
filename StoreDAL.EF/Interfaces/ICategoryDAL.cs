using System.Collections.Generic;
using StoreDTO;

namespace StoreDAL.Interfaces
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