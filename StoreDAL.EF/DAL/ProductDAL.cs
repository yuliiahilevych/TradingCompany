using System;
using System.Linq;
using StoreDAL.EF.Data;
using StoreDAL.EF.Entities;
using StoreDAL.Interfaces;
using StoreDTO;

namespace StoreDAL.EF.DAL
{
    public class ProductDAL : BaseDAL<tblProduct, ProductDTO>, IProductDAL
    {
        public ProductDAL(TradingCompanyContext context) : base(context)
        {
        }

        protected override int GetId(ProductDTO dto) => dto.ProductId;

        protected override IQueryable<tblProduct> ApplyOrder(IQueryable<tblProduct> query)
            => query.OrderBy(p => p.ProductId);

        // якщо змінилась ціна, автоматично додаємо запис в історію цін
        protected override void BeforeUpdate(tblProduct existing, ProductDTO newValues)
        {
            bool priceChanged = existing.PurchasePrice != newValues.PurchasePrice
                             || existing.SellingPrice != newValues.SellingPrice;
            if (!priceChanged) return;

            Context.tblPriceHistory.Add(new tblPriceHistory
            {
                ProductId = existing.ProductId,
                OldPurchasePrice = existing.PurchasePrice,   
                NewPurchasePrice = newValues.PurchasePrice,  
                OldSellingPrice = existing.SellingPrice,
                NewSellingPrice = newValues.SellingPrice,
                ChangedAt = DateTime.Now
            });
        }

        protected override ProductDTO ToDto(tblProduct e) => new ProductDTO
        {
            ProductId = e.ProductId,
            ProductName = e.ProductName,
            PurchasePrice = e.PurchasePrice,
            SellingPrice = e.SellingPrice,
            CategoryId = e.CategoryId,
            IsBlocked = e.IsBlocked
        };

        protected override tblProduct ToEntity(ProductDTO dto) => new tblProduct
        {
            ProductName = dto.ProductName,
            PurchasePrice = dto.PurchasePrice,
            SellingPrice = dto.SellingPrice,
            CategoryId = dto.CategoryId,
            IsBlocked = dto.IsBlocked
        };

        protected override void CopyToEntity(ProductDTO dto, tblProduct e)
        {
            e.ProductName = dto.ProductName;
            e.PurchasePrice = dto.PurchasePrice;
            e.SellingPrice = dto.SellingPrice;
            e.CategoryId = dto.CategoryId;
            e.IsBlocked = dto.IsBlocked;
        }
    }
}