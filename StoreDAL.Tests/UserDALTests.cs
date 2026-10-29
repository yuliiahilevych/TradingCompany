using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using StoreDAL.EF.DAL;
using StoreDAL.EF.Data;
using StoreDAL.EF.Entities;
using StoreDAL.EF.Interfaces;
using StoreDTO;

namespace StoreDAL.Tests
{
    [TestFixture]
    public class UserDALTests
    {
        private TradingCompanyContext _context = null!;
        private IUserDAL _dal = null!;
        private int _managerRoleId;
        private int _sellerRoleId;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<TradingCompanyContext>()
                .UseInMemoryDatabase("UserTests_" + Guid.NewGuid())
                .Options;

            _context = new TradingCompanyContext(options);
            _dal = new UserDAL(_context);

            //спершу створюємо ролі
            var manager = new tblRole { RoleName = "ProductManager" };
            var seller = new tblRole { RoleName = "Seller" };
            _context.tblRole.AddRange(manager, seller);
            _context.SaveChanges();
            _managerRoleId = manager.RoleId;
            _sellerRoleId = seller.RoleId;
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        private UserDTO AddUser(string login, int roleId)
        {
            return _dal.Create(new UserDTO
            {
                Login = login,
                PasswordHash = new byte[32],
                PasswordSalt = Guid.NewGuid(),
                RoleId = roleId,
                IsActive = true
            });
        }

        [Test]
        public void Create_ValidUser_ReturnsUserWithIdAndRoleName()
        {
            var created = AddUser("manager", _managerRoleId);

            Assert.That(created.UserId, Is.GreaterThan(0));
            Assert.That(created.RoleName, Is.EqualTo("ProductManager"));
        }

        [Test]
        public void GetAll_TwoUsers_ReturnsBoth()
        {
            AddUser("manager", _managerRoleId);
            AddUser("seller", _sellerRoleId);

            Assert.That(_dal.GetAll(), Has.Count.EqualTo(2));
        }

        [Test]
        public void GetByLogin_ExistingLogin_ReturnsUserWithRole()
        {
            AddUser("manager", _managerRoleId);
            AddUser("seller", _sellerRoleId);

            var found = _dal.GetByLogin("seller");

            Assert.That(found!.RoleName, Is.EqualTo("Seller"));
        }

        [Test]
        public void GetByLogin_UnknownLogin_ReturnsNull()
        {
            AddUser("manager", _managerRoleId);

            Assert.That(_dal.GetByLogin("nobody"), Is.Null);
        }

        [Test]
        public void Update_ExistingUser_ChangesFields()
        {
            var user = AddUser("manager", _managerRoleId);
            user.IsActive = false;

            var result = _dal.Update(user);

            Assert.That(result, Is.True);
            Assert.That(_dal.GetById(user.UserId)!.IsActive, Is.False);
        }

        [Test]
        public void Update_MissingUser_ReturnsFalse()
        {
            Assert.That(_dal.Update(new UserDTO { UserId = 999 }), Is.False);
        }

        [Test]
        public void Delete_ExistingUser_RemovesIt()
        {
            var user = AddUser("manager", _managerRoleId);

            _dal.Delete(user.UserId);

            Assert.That(_dal.GetById(user.UserId), Is.Null);
        }
    }
}