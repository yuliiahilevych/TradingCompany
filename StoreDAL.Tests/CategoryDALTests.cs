using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using StoreDAL.EF.DAL;
using StoreDAL.EF.Data;
using StoreDAL.Interfaces;
using StoreDTO;

namespace StoreDAL.Tests
{
    [TestFixture]
    public class CategoryDALTests
    {
        private TradingCompanyContext _context = null!;
        private ICategoryDAL _dal = null!;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<TradingCompanyContext>()
                .UseInMemoryDatabase("CategoryTests_" + Guid.NewGuid())
                .Options;

            _context = new TradingCompanyContext(options);
            _dal = new CategoryDAL(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        private CategoryDTO AddCategory(string name)
        {
            return _dal.Create(new CategoryDTO { CategoryName = name, Description = "Test description" });
        }


        [TestCase("Books")]
        [TestCase("Toys")]
        [TestCase("Home Appliances")]

        public void Create_ValidCategory_ReturnsCategoryWithId(string name)
        {
            var category = new CategoryDTO { CategoryName = name, Description = "Test" };

            var created = _dal.Create(category);

            Assert.Multiple(() =>
            {
                Assert.That(created.CategoryId, Is.GreaterThan(0));
                Assert.That(created.CategoryName, Is.EqualTo(name));
                Assert.That(_context.tblCategory.Count(), Is.EqualTo(1));
            });
        }


        [TestCase(1)]
        [TestCase(3)]
        [TestCase(5)]
        public void GetAll_SeveralCategories_ReturnsAll(int count)
        {
            for (int i = 1; i <= count; i++)
            {
                AddCategory("Category " + i);
            }

            var all = _dal.GetAll();

            Assert.That(all, Has.Count.EqualTo(count));
        }

        [Test]
        public void GetById_ExistingId_ReturnsCategory()
        {
            var created = AddCategory("Books");

            var found = _dal.GetById(created.CategoryId);

            Assert.Multiple(() =>
            {
                Assert.That(found, Is.Not.Null);
                Assert.That(found!.CategoryName, Is.EqualTo("Books"));
                Assert.That(found.Description, Is.EqualTo("Test description"));
            });
        }

        [Test]
        public void Update_ExistingCategory_ChangesName()
        {
            var created = AddCategory("Books");
            created.CategoryName = "E-Books";

            bool result = _dal.Update(created);
            var updated = _dal.GetById(created.CategoryId);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.True);
                Assert.That(updated!.CategoryName, Is.EqualTo("E-Books"));
            });
        }

        [Test]
        public void Delete_ExistingCategory_RemovesIt()
        {
            var created = AddCategory("Books");

            bool result = _dal.Delete(created.CategoryId);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.True);
                Assert.That(_dal.GetById(created.CategoryId), Is.Null);
            });
        }
    }
}