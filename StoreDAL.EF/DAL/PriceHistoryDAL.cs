using System.Collections.Generic;
using System.Linq;
using StoreDAL.EF.Data;
using StoreDAL.EF.Entities;
using StoreDAL.EF.Interfaces;
using StoreDTO;

namespace StoreDAL.EF.DAL
{
    public class PriceHistoryDAL : BaseDAL<tblPriceHistory>, IPriceHistoryDAL
    {
        public PriceHistoryDAL(TradingCompanyContext context) : base(context) { }

        public PriceHistoryDTO Create(PriceHistoryDTO history)
        {
            var entity = new tblPriceHistory
            {
                ProductId = history.ProductId,
                OldPurchasePrice = history.OldPurchasePrice,
                NewPurchasePrice = history.NewPurchasePrice,
                OldSellingPrice = history.OldSellingPrice,
                NewSellingPrice = history.NewSellingPrice
            };
            return ToDto(AddEntity(entity));
        }

        public List<PriceHistoryDTO> GetAll()
        {
            return GetAllEntities().Select(ToDto).ToList();
        }

        public PriceHistoryDTO? GetById(int id)
        {
            var entity = FindEntity(id);
            return entity == null ? null : ToDto(entity);
        }

        public bool Update(PriceHistoryDTO history)
        {
            var entity = FindEntity(history.PriceHistoryId);
            if (entity == null) return false;

            entity.ProductId = history.ProductId;
            entity.OldPurchasePrice = history.OldPurchasePrice;
            entity.NewPurchasePrice = history.NewPurchasePrice;
            entity.OldSellingPrice = history.OldSellingPrice;
            entity.NewSellingPrice = history.NewSellingPrice;
            SaveChanges();
            return true;
        }

        public bool Delete(int id)
        {
            return DeleteEntity(id);
        }

        private static PriceHistoryDTO ToDto(tblPriceHistory entity)
        {
            return new PriceHistoryDTO
            {
                PriceHistoryId = entity.PriceHistoryId,
                ProductId = entity.ProductId,
                OldPurchasePrice = entity.OldPurchasePrice,
                NewPurchasePrice = entity.NewPurchasePrice,
                OldSellingPrice = entity.OldSellingPrice,
                NewSellingPrice = entity.NewSellingPrice,
                RowInsertTime = entity.RowInsertTime
            };
        }
    }
}