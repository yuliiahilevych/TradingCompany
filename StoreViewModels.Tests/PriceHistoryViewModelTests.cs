using Moq;
using NUnit.Framework;
using StoreBL.Products;
using StoreDTO;
using StoreViewModels.Products;

namespace StoreViewModels.Tests
{
    [TestFixture]
    public class PriceHistoryViewModelTests
    {
        [Test]
        public void Load_FillsHistoryAndTitle()
        {
            var service = new Mock<IProductService>();
            service.Setup(s => s.GetPriceHistory(10)).Returns(new List<PriceHistoryDTO>
            {
                new PriceHistoryDTO { ProductId = 10, OldSellingPrice = 150, NewSellingPrice = 170 }
            });
            var viewModel = new PriceHistoryViewModel(service.Object);

            viewModel.Load(10, "Lego");

            Assert.That(viewModel.Title, Is.EqualTo("Історія цін: Lego"));
            Assert.That(viewModel.History, Has.Count.EqualTo(1));
            Assert.That(viewModel.IsEmpty, Is.False);
        }

        [Test]
        public void Load_NoChanges_IsEmpty()
        {
            var service = new Mock<IProductService>();
            service.Setup(s => s.GetPriceHistory(11)).Returns(new List<PriceHistoryDTO>());
            var viewModel = new PriceHistoryViewModel(service.Object);

            viewModel.Load(11, "Doll");

            Assert.That(viewModel.IsEmpty, Is.True);
        }
    }
}