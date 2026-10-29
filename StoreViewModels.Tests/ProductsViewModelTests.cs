using Moq;
using NUnit.Framework;
using StoreBL.Categories;
using StoreBL.Exceptions;
using StoreBL.Products;
using StoreDTO;
using StoreViewModels.Base;
using StoreViewModels.Products;

namespace StoreViewModels.Tests
{
    [TestFixture]
    public class ProductsViewModelTests
    {
        private Mock<IProductService> _productService = null!;
        private Mock<ICategoryService> _categoryService = null!;
        private Mock<IDialogService> _dialog = null!;
        private ProductsViewModel _viewModel = null!;

        [SetUp]
        public void SetUp()
        {
            _productService = new Mock<IProductService>();
            _categoryService = new Mock<ICategoryService>();
            _dialog = new Mock<IDialogService>();

            _categoryService.Setup(s => s.GetAll()).Returns(() => new List<CategoryDTO>
            {
                new CategoryDTO { CategoryId = 1, CategoryName = "Toys" },
                new CategoryDTO { CategoryId = 2, CategoryName = "Old", IsDeleted = true }
            });

            _productService.Setup(s => s.Search(It.IsAny<string?>(), It.IsAny<int?>())).Returns(() => new List<ProductDTO>
            {
                new ProductDTO { ProductId = 10, ProductName = "Lego", CategoryId = 1, PurchasePrice = 100, SellingPrice = 150 },
                new ProductDTO { ProductId = 20, ProductName = "Ball", CategoryId = 2, PurchasePrice = 10, SellingPrice = 20, IsBlocked = true }
            });

            _viewModel = new ProductsViewModel(_productService.Object, _categoryService.Object, _dialog.Object);
            _viewModel.Load();
        }

        [Test]
        public void FilterCategory_CallsSearchWithCategoryId()
        {
            _viewModel.FilterCategoryId = 1;

            _productService.Verify(s => s.Search(It.IsAny<string?>(), 1), Times.Once);
        }

        [Test]
        public void PriceNotNumber_ShowsErrorAndDisablesSave()
        {
            _viewModel.SelectedProduct = _viewModel.Products[0];

            _viewModel.Editor!.PurchasePriceText = "abc";

            Assert.That(_viewModel.Editor.GetErrors(nameof(ProductEditorViewModel.PurchasePriceText)), Is.Not.Empty);
            Assert.That(_viewModel.SaveCommand.CanExecute(null), Is.False);
        }

        [Test]
        public void SaveCommand_ChangedPriceWithComma_CallsUpdate()
        {
            _productService.Setup(s => s.Update(It.IsAny<ProductDTO>())).Returns(true);
            _viewModel.SelectedProduct = _viewModel.Products[0];

            _viewModel.Editor!.PurchasePriceText = "120,50";
            _viewModel.SaveCommand.Execute(null);

            _productService.Verify(s => s.Update(It.Is<ProductDTO>(p =>
                p.ProductId == 10 && p.PurchasePrice == 120.50m)), Times.Once);
        }

        [Test]
        public void BlockCommand_Confirmed_CallsBlock()
        {
            _viewModel.SelectedProduct = _viewModel.Products[0];
            _dialog.Setup(d => d.Confirm(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

            _viewModel.BlockCommand.Execute(null);

            _productService.Verify(s => s.Block(10), Times.Once);
        }


        [Test]
        public void DeleteCommand_OnlyForBlockedProduct()
        {
            _viewModel.SelectedProduct = _viewModel.Products[0];          
            Assert.That(_viewModel.DeleteCommand.CanExecute(null), Is.False);

            _viewModel.SelectedProduct = _viewModel.Products[1];          
            Assert.That(_viewModel.DeleteCommand.CanExecute(null), Is.True);
        }
    }
}