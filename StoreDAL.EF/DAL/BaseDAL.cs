using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using StoreDAL.EF.Data;

namespace StoreDAL.EF.DAL
{
   
    public abstract class BaseDAL<TEntity> where TEntity : class
    {
        protected readonly TradingCompanyContext _context;

        protected BaseDAL(TradingCompanyContext context)
        {
            _context = context;
        }


        protected DbSet<TEntity> Table => _context.Set<TEntity>();

        protected TEntity AddEntity(TEntity entity)
        {
            Table.Add(entity);
            _context.SaveChanges();
            return entity;
        }

        protected List<TEntity> GetAllEntities()
        {
            return Table.AsNoTracking().ToList();
        }

        protected TEntity? FindEntity(int id)
        {
            return Table.Find(id);
        }

        protected void SaveChanges()
        {
            _context.SaveChanges();
        }

        protected bool DeleteEntity(int id)
        {
            var entity = Table.Find(id);
            if (entity == null) return false;

            Table.Remove(entity);
            _context.SaveChanges();
            return true;
        }
    }
}