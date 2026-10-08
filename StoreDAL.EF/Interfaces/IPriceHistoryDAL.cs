using System.Collections.Generic;
using StoreDTO;

namespace StoreDAL.Interfaces
{
    public interface IPriceHistoryDAL
    {
        PriceHistoryDTO Create(PriceHistoryDTO history);
        List<PriceHistoryDTO> GetAll();
        PriceHistoryDTO? GetById(int id);
        bool Update(PriceHistoryDTO history);
        bool Delete(int id);
    }
}