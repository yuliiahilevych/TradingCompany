using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using StoreDAL.EF.Data;
using StoreDAL.EF.Entities;
using StoreDAL.EF.Interfaces;
using StoreDTO;

namespace StoreDAL.EF.DAL
{
    public class UserDAL : BaseDAL<tblUser>, IUserDAL
    {
        public UserDAL(TradingCompanyContext context) : 
            base(context) 
        { }

        public UserDTO Create(UserDTO user)
        {
            var entity = new tblUser
            {
                Login = user.Login,
                PasswordHash = user.PasswordHash,
                PasswordSalt = user.PasswordSalt,
                RoleId = user.RoleId,
                IsActive = user.IsActive
            };
            AddEntity(entity);

            _context.Entry(entity).Reference(e => e.Role).Load();
            return ToDto(entity);
        }

        public List<UserDTO> GetAll()
        {
            return UsersWithRole().Select(ToDto).ToList();
        }

        public UserDTO? GetById(int id)
        {
            var entity = UsersWithRole().FirstOrDefault(u => u.UserId == id);
            return entity == null ? null : ToDto(entity);
        }

        public UserDTO? GetByLogin(string login)
        {
            var entity = UsersWithRole().FirstOrDefault(u => u.Login == login);
            return entity == null ? null : ToDto(entity);
        }

        public bool Update(UserDTO user)
        {
            var entity = FindEntity(user.UserId);
            if (entity == null) return false;

            entity.Login = user.Login;
            entity.PasswordHash = user.PasswordHash;
            entity.PasswordSalt = user.PasswordSalt;
            entity.RoleId = user.RoleId;
            entity.IsActive = user.IsActive;
            SaveChanges();
            return true;
        }

        public bool Delete(int id)
        {
            return DeleteEntity(id);
        }

        // Користувачі разом із їхньою роллю (JOIN з tblRole)
        private IQueryable<tblUser> UsersWithRole()
        {
            return Table.AsNoTracking().Include(u => u.Role);
        }

        private static UserDTO ToDto(tblUser entity)
        {
            return new UserDTO
            {
                UserId = entity.UserId,
                Login = entity.Login,
                PasswordHash = entity.PasswordHash,
                PasswordSalt = entity.PasswordSalt,
                RoleId = entity.RoleId,
                RoleName = entity.Role?.RoleName ?? string.Empty,
                IsActive = entity.IsActive,
                RowInsertTime = entity.RowInsertTime
            };
        }
    }
}