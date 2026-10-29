using Moq;
using NUnit.Framework;
using StoreBL.Categories;
using StoreBL.Exceptions;
using StoreDTO;
using StoreViewModels.Base;
using StoreViewModels.Categories;

namespace StoreViewModels.Tests
{
    [TestFixture]
    public class CategoriesViewModelTests
    {
        private Mock<ICategoryService> _service = null!;
        private Mock<IDialogService> _dialog = null!;
        private CategoriesViewModel _viewModel = null!;

        [SetUp]
        public void SetUp()
        {
            _service = new Mock<ICategoryService>();
            _dialog = new Mock<IDialogService>();

            _service.Setup(s => s.GetAll()).Returns(() => new List<CategoryDTO>
            {
                new CategoryDTO { CategoryId = 1, CategoryName = "Books" },
                new CategoryDTO { CategoryId = 2, CategoryName = "Old", IsDeleted = true }
            });

            _viewModel = new CategoriesViewModel(_service.Object, _dialog.Object);
            _viewModel.Load();
        }

        [Test]
        public void Load_FillsCategories()
        {
            Assert.That(_viewModel.Categories.Select(c => c.CategoryName), Is.EqualTo(new[] { "Books", "Old" }));
            Assert.That(_viewModel.Editor, Is.Null);
        }

        [Test]
        public void EditingInEditor_DoesNotChangeListItem()
        {
            _viewModel.SelectedCategory = _viewModel.Categories[0];

            _viewModel.Editor!.CategoryName = "Changed";

            Assert.That(_viewModel.Categories[0].CategoryName, Is.EqualTo("Books"));   
        }

        [Test]
        public void SaveCommand_NewCategory_CallsCreate()
        {
            _service.Setup(s => s.Create(It.IsAny<CategoryDTO>())).Returns(new CategoryDTO { CategoryId = 3 });
            _viewModel.AddCommand.Execute(null);
            _viewModel.Editor!.CategoryName = "Toys";

            _viewModel.SaveCommand.Execute(null);

            _service.Verify(s => s.Create(It.Is<CategoryDTO>(c => c.CategoryName == "Toys")), Times.Once);
        }

        [Test]
        public void MarkDeletedCommand_NotConfirmed_DoesNothing()
        {
            _viewModel.SelectedCategory = _viewModel.Categories[0];
            _dialog.Setup(d => d.Confirm(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

            _viewModel.MarkDeletedCommand.Execute(null);

            _service.Verify(s => s.MarkDeleted(It.IsAny<int>()), Times.Never);
        }

        [Test]
        public void DeleteCommand_Confirmed_CallsServiceAndShowsMessage()
        {
            _viewModel.SelectedCategory = _viewModel.Categories[0];
            _dialog.Setup(d => d.Confirm(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

            _viewModel.DeleteCommand.Execute(null);

            _service.Verify(s => s.Delete(1), Times.Once);
            Assert.That(_viewModel.StatusMessage, Does.Contain("видалено"));
        }
    }
}