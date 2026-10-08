using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using StoreDAL.EF.DAL;
using StoreDAL.EF.Data;
using StoreDAL.EF.Entities;
using StoreDAL.Interfaces;
using StoreDTO;

namespace StoreDAL.Tests
{
    [TestFixture]
    public class ProductDALTests
    {
        private TradingCompanyContext _context = null!;
        private IProductDAL _dal = null!;
        private int _categoryId;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<TradingCompanyContext>()
                .UseInMemoryDatabase("ProductTests_" + Guid.NewGuid())
                .Options;

            _context = new TradingCompanyContext(options);
            _dal = new ProductDAL(_context);

            var category = new tblCategory { CategoryName = "Test category" };
            _context.tblCategory.Add(category);
            _context.SaveChanges();
            _categoryId = category.CategoryId;
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        private ProductDTO AddProduct(string name, decimal purchase = 100, decimal selling = 150)
        {
            return _dal.Create(new ProductDTO
            {
                ProductName = name,
                PurchasePrice = purchase,
                SellingPrice = selling,
                CategoryId = _categoryId
            });
        }

 
        [TestCase("LEGO City", 1450.0, 1999.0)]
        [TestCase("Yoga Mat", 320.0, 499.0)]
        [TestCase("Pilot Ballpoint Pen", 18.5, 32.0)]
        public void Create_ValidProduct_ReturnsProductWithId(string name, double purchase, double selling)
        {
            var product = new ProductDTO
            {
                ProductName = name,
                PurchasePrice = (decimal)purchase,
                SellingPrice = (decimal)selling,
                CategoryId = _categoryId
            };
            
            var created = _dal.Create(product);

            Assert.Multiple(() =>
            {
                Assert.That(created.ProductId, Is.GreaterThan(0));
                Assert.That(created.ProductName, Is.EqualTo(name));
                Assert.That(created.PurchasePrice, Is.EqualTo((decimal)purchase));
                Assert.That(created.SellingPrice, Is.EqualTo((decimal)selling));
                Assert.That(created.IsBlocked, Is.False);
            });
        }


        [Test]
        public void GetAll_ThreeProducts_ReturnsAll()
        {
            AddProduct("LEGO");
            AddProduct("Puzzle");
            AddProduct("Ball");

            var all = _dal.GetAll();

            Assert.That(all, Has.Count.EqualTo(3));
        }


        [Test]
        public void GetById_ExistingId_ReturnsProduct()
        {
            var created = AddProduct("LEGO");

            var found = _dal.GetById(created.ProductId);

            Assert.Multiple(() =>
            {
                Assert.That(found, Is.Not.Null);
                Assert.That(found!.ProductName, Is.EqualTo("LEGO"));
                Assert.That(found.CategoryId, Is.EqualTo(_categoryId));
            });
        }


        [TestCase(100.0, 150.0, 120.0, 199.0)]
        [TestCase(100.0, 150.0, 90.0, 140.0)]
        [TestCase(100.0, 150.0, 100.0, 175.0)]
        public void Update_PriceChanged_AddsPriceHistoryRecord(
            double oldPurchase, double oldSelling, double newPurchase, double newSelling)
        {
            var created = AddProduct("LEGO", (decimal)oldPurchase, (decimal)oldSelling);
            created.PurchasePrice = (decimal)newPurchase;
            created.SellingPrice = (decimal)newSelling;

            bool result = _dal.Update(created);

            var history = _context.tblPriceHistory.Where(h => h.ProductId == created.ProductId).ToList();
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.True);
                Assert.That(history, Has.Count.EqualTo(1));
                Assert.That(history[0].OldPurchasePrice, Is.EqualTo((decimal)oldPurchase));
                Assert.That(history[0].NewPurchasePrice, Is.EqualTo((decimal)newPurchase));
                Assert.That(history[0].OldSellingPrice, Is.EqualTo((decimal)oldSelling));
                Assert.That(history[0].NewSellingPrice, Is.EqualTo((decimal)newSelling));
            });
        }


        [Test]
        public void Delete_ExistingProduct_RemovesIt()
        {
            var created = AddProduct("LEGO");

            bool result = _dal.Delete(created.ProductId);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.True);
                Assert.That(_dal.GetById(created.ProductId), Is.Null);
            });
        }
    }
}