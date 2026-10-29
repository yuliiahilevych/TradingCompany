using Moq;
using NUnit.Framework;
using StoreBL.Exceptions;
using StoreBL.Products;
using StoreBL.Users;
using StoreDAL.EF.Interfaces;
using StoreDTO;

namespace StoreBL.Tests
{
    [TestFixture]
    public class ProductServiceTests
    {
        private Mock<IProductDAL> _productDal = null!;
        private Mock<ICategoryDAL> _categoryDal = null!;
        private Mock<IPriceHistoryDAL> _historyDal = null!;
        private Mock<IUserSession> _session = null!;
        private ProductService _service = null!;

        [SetUp]
        public void SetUp()
        {
            _productDal = new Mock<IProductDAL>();
            _categoryDal = new Mock<ICategoryDAL>();
            _historyDal = new Mock<IPriceHistoryDAL>();
            _session = new Mock<IUserSession>();

            _session.Setup(s => s.IsProductManager).Returns(true);

            _categoryDal.Setup(d => d.GetById(1)).Returns(new CategoryDTO { CategoryId = 1, CategoryName = "Toys" });
            _categoryDal.Setup(d => d.GetById(2)).Returns(new CategoryDTO { CategoryId = 2, CategoryName = "Old", IsDeleted = true });
            _categoryDal.Setup(d => d.GetById(3)).Returns(new CategoryDTO { CategoryId = 3, CategoryName = "Games" });

            _productDal.Setup(d => d.GetAll()).Returns(() => new List<ProductDTO>
            {
                new ProductDTO { ProductId = 10, ProductName = "Lego", CategoryId = 1, PurchasePrice = 100, SellingPrice = 150 },
                new ProductDTO { ProductId = 11, ProductName = "Doll", CategoryId = 1, PurchasePrice = 50,  SellingPrice = 80 },
                new ProductDTO { ProductId = 20, ProductName = "Ball", CategoryId = 2, PurchasePrice = 10,  SellingPrice = 20, IsBlocked = true }
            });
            _productDal.Setup(d => d.GetById(10)).Returns(() =>
                new ProductDTO { ProductId = 10, ProductName = "Lego", CategoryId = 1, PurchasePrice = 100, SellingPrice = 150 });
            _productDal.Setup(d => d.GetById(20)).Returns(() =>
                new ProductDTO { ProductId = 20, ProductName = "Ball", CategoryId = 2, PurchasePrice = 10, SellingPrice = 20, IsBlocked = true });

            _productDal.Setup(d => d.Create(It.IsAny<ProductDTO>())).Returns<ProductDTO>(p => p);
            _productDal.Setup(d => d.Update(It.IsAny<ProductDTO>())).Returns(true);

            _service = new ProductService(_productDal.Object, _categoryDal.Object, _historyDal.Object, _session.Object);
        }

        [Test]
        public void Create_ValidProduct_TrimsNameAndDoesNotChangeInput()
        {
            ProductDTO? saved = null;
            _productDal.Setup(d => d.Create(It.IsAny<ProductDTO>()))
                .Callback<ProductDTO>(p => saved = p)
                .Returns<ProductDTO>(p => p);
            var input = new ProductDTO { ProductName = "  Puzzle  ", CategoryId = 1, PurchasePrice = 30, SellingPrice = 45 };

            _service.Create(input);

            Assert.That(saved!.ProductName, Is.EqualTo("Puzzle"));
            Assert.That(input.ProductName, Is.EqualTo("  Puzzle  "));
        }

        [Test]
        public void Create_SameNameInSameCategory_Throws()
        {
            var product = new ProductDTO { ProductName = "lego", CategoryId = 1, PurchasePrice = 1, SellingPrice = 2 };

            var ex = Assert.Throws<BusinessRuleException>(() => _service.Create(product));

            Assert.That(ex!.Message, Does.Contain("вже є в цій категорії"));
        }


        [Test]
        public void Delete_NotBlockedProduct_ThrowsAndDoesNotCallDal()
        {
            Assert.Throws<BusinessRuleException>(() => _service.Delete(10));   

            _productDal.Verify(d => d.Delete(It.IsAny<int>()), Times.Never);
        }

        [Test]
        public void Delete_BlockedProduct_DeletesHistoryAndProduct()
        {
            _historyDal.Setup(d => d.GetAll()).Returns(new List<PriceHistoryDTO>
            {
                new PriceHistoryDTO { PriceHistoryId = 5, ProductId = 20 },
                new PriceHistoryDTO { PriceHistoryId = 6, ProductId = 10 } 
            });

            _service.Delete(20);  

            _historyDal.Verify(d => d.Delete(5), Times.Once);
            _historyDal.Verify(d => d.Delete(6), Times.Never);
            _productDal.Verify(d => d.Delete(20), Times.Once);
        }
    }
}
