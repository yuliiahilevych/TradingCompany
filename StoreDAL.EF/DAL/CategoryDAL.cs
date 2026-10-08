using System.Linq;
using StoreDAL.EF.Data;
using StoreDAL.EF.Entities;
using StoreDAL.Interfaces;
using StoreDTO;

namespace StoreDAL.EF.DAL
{
    public class CategoryDAL : BaseDAL<tblCategory, CategoryDTO>, ICategoryDAL
    {
        public CategoryDAL(TradingCompanyContext context) : base(context)
        {
        }

        protected override int GetId(CategoryDTO dto) => dto.CategoryId;

        protected override IQueryable<tblCategory> ApplyOrder(IQueryable<tblCategory> query)
            => query.OrderBy(c => c.CategoryId);

        // якщо категорію позначили видаленою, блокуємо всі її товари
        protected override void BeforeUpdate(tblCategory existing, CategoryDTO newValues)
        {
            bool becameDeleted = !existing.IsDeleted && newValues.IsDeleted;
            if (!becameDeleted) return;

            var products = Context.tblProduct
                .Where(p => p.CategoryId == existing.CategoryId && !p.IsBlocked)
                .ToList();

            foreach (var p in products)
            {
                p.IsBlocked = true;
            }
        }

        protected override CategoryDTO ToDto(tblCategory e) => new CategoryDTO
        {
            CategoryId = e.CategoryId,
            CategoryName = e.CategoryName,
            IsDeleted = e.IsDeleted,
            Description = e.Description
        };

        protected override tblCategory ToEntity(CategoryDTO dto) => new tblCategory
        {
            CategoryName = dto.CategoryName,
            IsDeleted = dto.IsDeleted,
            Description = dto.Description
        };

        protected override void CopyToEntity(CategoryDTO dto, tblCategory e)
        {
            e.CategoryName = dto.CategoryName;
            e.IsDeleted = dto.IsDeleted;
            e.Description = dto.Description;
        }
    }
}