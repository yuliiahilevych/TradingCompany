using System.Collections.Generic;
using System.Linq;
using StoreDAL.EF.Data;
using StoreDAL.EF.Entities;
using StoreDAL.EF.Interfaces;
using StoreDTO;

namespace StoreDAL.EF.DAL
{
    public class CategoryDAL : BaseDAL<tblCategory>, ICategoryDAL
    {
        public CategoryDAL(TradingCompanyContext context) : base(context) { }

        public CategoryDTO Create(CategoryDTO category)
        {
            var entity = new tblCategory
            {
                CategoryName = category.CategoryName,
                IsDeleted = category.IsDeleted,
                Description = category.Description
            };
            return ToDto(AddEntity(entity));
        }

        public List<CategoryDTO> GetAll()
        {
            return GetAllEntities().Select(ToDto).ToList();
        }

        public CategoryDTO? GetById(int id)
        {
            var entity = FindEntity(id);
            return entity == null ? null : ToDto(entity);
        }

        public bool Update(CategoryDTO category)
        {
            var entity = FindEntity(category.CategoryId);
            if (entity == null) return false;

            entity.CategoryName = category.CategoryName;
            entity.IsDeleted = category.IsDeleted;
            entity.Description = category.Description;
            SaveChanges();
            return true;
        }

        public bool Delete(int id)
        {
            return DeleteEntity(id);
        }

        private static CategoryDTO ToDto(tblCategory entity)
        {
            return new CategoryDTO
            {
                CategoryId = entity.CategoryId,
                CategoryName = entity.CategoryName,
                IsDeleted = entity.IsDeleted,
                Description = entity.Description,
                RowInsertTime = entity.RowInsertTime
            };
        }
    }
}