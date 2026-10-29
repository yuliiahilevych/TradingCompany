using System;
using System.Collections.Generic;
using System.Linq;
using StoreDAL.EF.Data;
using StoreDAL.EF.Entities;
using StoreDAL.EF.Interfaces;
using StoreDTO;

namespace StoreDAL.EF.DAL
{
    public class ProductDAL : BaseDAL<tblProduct>, IProductDAL
    {
        public ProductDAL(TradingCompanyContext context) : base(context) { }

        public ProductDTO Create(ProductDTO product)
        {
            var entity = new tblProduct
            {
                ProductName = product.ProductName,
                PurchasePrice = product.PurchasePrice,
                SellingPrice = product.SellingPrice,
                CategoryId = product.CategoryId,
                IsBlocked = product.IsBlocked
            };
            return ToDto(AddEntity(entity));
        }

        public List<ProductDTO> GetAll()
        {
            return GetAllEntities().Select(ToDto).ToList();
        }

        public ProductDTO? GetById(int id)
        {
            var entity = FindEntity(id);
            return entity == null ? null : ToDto(entity);
        }

        public bool Update(ProductDTO product)
        {
            var entity = FindEntity(product.ProductId);
            if (entity == null) return false;

            entity.ProductName = product.ProductName;
            entity.PurchasePrice = product.PurchasePrice;
            entity.SellingPrice = product.SellingPrice;
            entity.CategoryId = product.CategoryId;
            entity.IsBlocked = product.IsBlocked;
            entity.RowUpdateTime = DateTime.Now;
            SaveChanges();
            return true;
        }

        public bool Delete(int id)
        {
            return DeleteEntity(id);
        }

        private static ProductDTO ToDto(tblProduct entity)
        {
            return new ProductDTO
            {
                ProductId = entity.ProductId,
                ProductName = entity.ProductName,
                PurchasePrice = entity.PurchasePrice,
                SellingPrice = entity.SellingPrice,
                CategoryId = entity.CategoryId,
                IsBlocked = entity.IsBlocked,
                RowInsertTime = entity.RowInsertTime,
                RowUpdateTime = entity.RowUpdateTime
            };
        }
    }
}