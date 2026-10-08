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
    public class PriceHistoryDALTests
    {
        private TradingCompanyContext _context = null!;
        private IPriceHistoryDAL _dal = null!;
        private int _productId;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<TradingCompanyContext>()
                .UseInMemoryDatabase("PriceHistoryTests_" + Guid.NewGuid())
                .Options;

            _context = new TradingCompanyContext(options);
            _dal = new PriceHistoryDAL(_context);

            var category = new tblCategory { CategoryName = "Test category" };
            _context.tblCategory.Add(category);
            _context.SaveChanges();

            var product = new tblProduct
            {
                ProductName = "Test product",
                PurchasePrice = 100,
                SellingPrice = 150,
                CategoryId = category.CategoryId
            };
            _context.tblProduct.Add(product);
            _context.SaveChanges();
            _productId = product.ProductId;
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        private PriceHistoryDTO AddRecord(decimal oldSell = 150, decimal newSell = 199)
        {
            return _dal.Create(new PriceHistoryDTO
            {
                ProductId = _productId,
                OldPurchasePrice = 100,
                NewPurchasePrice = 120,
                OldSellingPrice = oldSell,
                NewSellingPrice = newSell
            });
        }


        [Test]
        public void Create_WithoutDate_ReturnsRecordWithIdAndCurrentDate()
        {
            var created = AddRecord();

            Assert.Multiple(() =>
            {
                Assert.That(created.PriceHistoryId, Is.GreaterThan(0));
                Assert.That(created.ProductId, Is.EqualTo(_productId));
                Assert.That(created.NewSellingPrice, Is.EqualTo(199m));
                Assert.That(created.ChangedAt, Is.EqualTo(DateTime.Now).Within(TimeSpan.FromMinutes(1)));
            });
        }


        [TestCase(1)]
        [TestCase(3)]
        [TestCase(5)]
        public void GetAll_SeveralRecords_ReturnsAll(int count)
        {
            for (int i = 0; i < count; i++)
            {
                AddRecord(150 + i, 160 + i);
            }

            var all = _dal.GetAll();

            Assert.That(all, Has.Count.EqualTo(count));
        }


        [Test]
        public void GetById_ExistingId_ReturnsRecord()
        {
            var created = AddRecord();

            var found = _dal.GetById(created.PriceHistoryId);

            Assert.Multiple(() =>
            {
                Assert.That(found, Is.Not.Null);
                Assert.That(found!.ProductId, Is.EqualTo(_productId));
                Assert.That(found.OldSellingPrice, Is.EqualTo(150m));
            });
        }


        [Test]
        public void Update_ExistingRecord_ChangesPrice()
        {
            var created = AddRecord();
            created.NewSellingPrice = 250;

            bool result = _dal.Update(created);
            var updated = _dal.GetById(created.PriceHistoryId);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.True);
                Assert.That(updated!.NewSellingPrice, Is.EqualTo(250m));
            });
        }


        [Test]
        public void Delete_ExistingRecord_RemovesIt()
        {
            var created = AddRecord();

            bool result = _dal.Delete(created.PriceHistoryId);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.True);
                Assert.That(_dal.GetById(created.PriceHistoryId), Is.Null);
            });
        }
    }
}