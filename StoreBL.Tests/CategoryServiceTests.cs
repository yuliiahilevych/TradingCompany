using Moq;
using NUnit.Framework;
using StoreBL.Categories;
using StoreBL.Exceptions;
using StoreBL.Users;
using StoreDAL.EF.Interfaces;
using StoreDTO;

namespace StoreBL.Tests
{
    [TestFixture]
    public class CategoryServiceTests
    {
        private Mock<ICategoryDAL> _categoryDal = null!;
        private Mock<IProductDAL> _productDal = null!;
        private Mock<IUserSession> _session = null!;
        private CategoryService _service = null!;

        [SetUp]
        public void SetUp()
        {
            _categoryDal = new Mock<ICategoryDAL>();
            _productDal = new Mock<IProductDAL>();
            _session = new Mock<IUserSession>();

            _session.Setup(s => s.IsProductManager).Returns(true);
            _categoryDal.Setup(d => d.GetAll()).Returns(() =>
                new List<CategoryDTO> { new CategoryDTO { CategoryId = 1, CategoryName = "Toys" } });
            _categoryDal.Setup(d => d.GetById(1)).Returns(() =>
                new CategoryDTO { CategoryId = 1, CategoryName = "Toys" });
            _categoryDal.Setup(d => d.Create(It.IsAny<CategoryDTO>())).Returns<CategoryDTO>(c => c);

            _service = new CategoryService(_categoryDal.Object, _productDal.Object, _session.Object);
        }

        [TestCase("")]
        [TestCase("   ")]
        public void Create_EmptyName_ThrowsAndDoesNotCallDal(string name)
        {
            var ex = Assert.Throws<BusinessRuleException>(
                () => _service.Create(new CategoryDTO { CategoryName = name }));

            Assert.That(ex!.Errors, Has.Some.Contains("обов'язкова"));
            _categoryDal.Verify(d => d.Create(It.IsAny<CategoryDTO>()), Times.Never);
        }

        [TestCase(50, true)]
        [TestCase(51, false)]
        public void Create_NameLengthBoundary(int length, bool isValid)
        {
            var category = new CategoryDTO { CategoryName = new string('a', length) };

            if (isValid)
                Assert.DoesNotThrow(() => _service.Create(category));
            else
                Assert.Throws<BusinessRuleException>(() => _service.Create(category));
        }

        [Test]
        public void Create_DuplicateNameIgnoringCase_Throws()
        {
            var ex = Assert.Throws<BusinessRuleException>(
                () => _service.Create(new CategoryDTO { CategoryName = "toys" }));

            Assert.That(ex!.Message, Does.Contain("вже існує"));
            _categoryDal.Verify(d => d.Create(It.IsAny<CategoryDTO>()), Times.Never);
        }

        [Test]
        public void MarkDeleted_BlocksAllProductsOfThisCategory()
        {
            _productDal.Setup(d => d.GetAll()).Returns(new List<ProductDTO>
            {
                new ProductDTO { ProductId = 10, CategoryId = 1, IsBlocked = false },
                new ProductDTO { ProductId = 11, CategoryId = 1, IsBlocked = false },
                new ProductDTO { ProductId = 20, CategoryId = 2, IsBlocked = false }   
            });

            var result = _service.MarkDeleted(1);

            Assert.That(result, Is.True);
            _categoryDal.Verify(d => d.Update(It.Is<CategoryDTO>(c => c.CategoryId == 1 && c.IsDeleted)), Times.Once);
            _productDal.Verify(d => d.Update(It.Is<ProductDTO>(p => p.CategoryId == 1 && p.IsBlocked)), Times.Exactly(2));
            _productDal.Verify(d => d.Update(It.Is<ProductDTO>(p => p.CategoryId == 2)), Times.Never);
        }

        [Test]
        public void Restore_DoesNotUnblockProducts()
        {
            _service.Restore(1);

            _categoryDal.Verify(d => d.Update(It.Is<CategoryDTO>(c => c.CategoryId == 1 && !c.IsDeleted)), Times.Once);
            _productDal.Verify(d => d.Update(It.IsAny<ProductDTO>()), Times.Never);
        }

        [Test]
        public void Create_NotProductManager_ThrowsAndDoesNotCallDal()
        {
            _session.Setup(s => s.IsProductManager).Returns(false);

            Assert.Throws<BusinessRuleException>(
                () => _service.Create(new CategoryDTO { CategoryName = "Books" }));

            _categoryDal.Verify(d => d.Create(It.IsAny<CategoryDTO>()), Times.Never);
        }


        [Test]
        public void Delete_CategoryWithProducts_ThrowsAndDoesNotCallDal()
        {
            _productDal.Setup(d => d.GetAll()).Returns(new List<ProductDTO>
            {
                new ProductDTO { ProductId = 10, CategoryId = 1 }
            });

            var ex = Assert.Throws<BusinessRuleException>(() => _service.Delete(1));

            Assert.That(ex!.Message, Does.Contain("є товари"));
            _categoryDal.Verify(d => d.Delete(It.IsAny<int>()), Times.Never);
        }

        [Test]
        public void Delete_EmptyCategory_CallsDal()
        {
            _productDal.Setup(d => d.GetAll()).Returns(new List<ProductDTO>());

            _service.Delete(1);

            _categoryDal.Verify(d => d.Delete(1), Times.Once);
        }
    }
}