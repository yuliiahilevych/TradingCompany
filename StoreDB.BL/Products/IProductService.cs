using StoreDTO;

namespace StoreBL.Products
{
    public interface IProductService
    {
        List<ProductDTO> GetAll();
        ProductDTO? GetById(int productId);

        List<ProductDTO> Search(string? text, int? categoryId);

        ProductDTO Create(ProductDTO product);

        bool Update(ProductDTO product);

        bool Block(int productId);
        bool Unblock(int productId);

        bool Delete(int productId);

        List<PriceHistoryDTO> GetPriceHistory(int productId);


    }
}