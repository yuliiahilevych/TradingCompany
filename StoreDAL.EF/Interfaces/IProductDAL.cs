using System.Collections.Generic;
using StoreDTO;

namespace StoreDAL.Interfaces
{
    public interface IProductDAL
    {
        ProductDTO Create(ProductDTO product);
        List<ProductDTO> GetAll();
        ProductDTO? GetById(int id);
        bool Update(ProductDTO product);
        bool Delete(int id);
    }
}
