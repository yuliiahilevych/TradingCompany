using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using StoreDAL.EF.Data;

namespace StoreDAL.EF.DAL
{
    public abstract class BaseDAL<TEntity, TDto>
        where TEntity : class
        where TDto : class
    {
        protected readonly TradingCompanyContext Context;

        protected DbSet<TEntity> Set => Context.Set<TEntity>();

        protected BaseDAL(TradingCompanyContext context)
        {
            Context = context;
        }

        protected abstract TDto ToDto(TEntity entity);                    // entity -> DTO
        protected abstract TEntity ToEntity(TDto dto);                    // DTO -> нова entity
        protected abstract void CopyToEntity(TDto dto, TEntity entity);   // DTO -> наявна entity
        protected abstract int GetId(TDto dto);                           // PK DTO


        protected virtual IQueryable<TEntity> ApplyOrder(IQueryable<TEntity> query) => query;

        protected virtual void BeforeUpdate(TEntity existing, TDto newValues) { }

        

        //
        public virtual TDto Create(TDto dto)
        {
            var entity = ToEntity(dto);
            Set.Add(entity);
            Context.SaveChanges();
            return ToDto(entity);
        }

        public virtual List<TDto> GetAll()
        {
            return ApplyOrder(Set.AsNoTracking())
                .ToList()
                .Select(ToDto)
                .ToList();
        }

        public virtual TDto? GetById(int id)
        {
            var entity = Set.Find(id);
            return entity == null ? null : ToDto(entity);
        }

        public virtual bool Update(TDto dto)
        {
            var entity = Set.Find(GetId(dto));
            if (entity == null) return false;

            BeforeUpdate(entity, dto);   // бачить старі значення
            CopyToEntity(dto, entity);   // потім переносимо нові значення

            Context.SaveChanges();
            return true;
        }

        public virtual bool Delete(int id)
        {
            var entity = Set.Find(id);
            if (entity == null) return false;

            Set.Remove(entity);
            Context.SaveChanges();
            return true;
        }
    }
}