using System;
using System.Linq;
using StoreDAL.EF.Data;
using StoreDAL.EF.Entities;
using StoreDAL.Interfaces;
using StoreDTO;

namespace StoreDAL.EF.DAL
{
    public class PriceHistoryDAL : BaseDAL<tblPriceHistory, PriceHistoryDTO>, IPriceHistoryDAL
    {
        public PriceHistoryDAL(TradingCompanyContext context) : base(context)
        {
        }

        protected override int GetId(PriceHistoryDTO dto) => dto.PriceHistoryId;

        // новіші записи першими
        protected override IQueryable<tblPriceHistory> ApplyOrder(IQueryable<tblPriceHistory> query)
            => query.OrderByDescending(h => h.ChangedAt);

        protected override PriceHistoryDTO ToDto(tblPriceHistory e) => new PriceHistoryDTO
        {
            PriceHistoryId = e.PriceHistoryId,
            ProductId = e.ProductId,
            OldPurchasePrice = e.OldPurchasePrice,
            NewPurchasePrice = e.NewPurchasePrice,
            OldSellingPrice = e.OldSellingPrice,
            NewSellingPrice = e.NewSellingPrice,
            ChangedAt = e.ChangedAt
        };

        protected override tblPriceHistory ToEntity(PriceHistoryDTO dto) => new tblPriceHistory
        {
            ProductId = dto.ProductId,
            OldPurchasePrice = dto.OldPurchasePrice,
            NewPurchasePrice = dto.NewPurchasePrice,
            OldSellingPrice = dto.OldSellingPrice,
            NewSellingPrice = dto.NewSellingPrice,
            ChangedAt = dto.ChangedAt == default ? DateTime.Now : dto.ChangedAt
        };

        protected override void CopyToEntity(PriceHistoryDTO dto, tblPriceHistory e)
        {
            e.ProductId = dto.ProductId;
            e.OldPurchasePrice = dto.OldPurchasePrice;
            e.NewPurchasePrice = dto.NewPurchasePrice;
            e.OldSellingPrice = dto.OldSellingPrice;
            e.NewSellingPrice = dto.NewSellingPrice;
            e.ChangedAt = dto.ChangedAt;
        }
    }
}