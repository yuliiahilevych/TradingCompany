using Moq;
using NUnit.Framework;
using StoreBL.Users;
using StoreDTO;
using StoreViewModels.Main;

namespace StoreViewModels.Tests
{
    [TestFixture]
    public class MainViewModelTests
    {
        private Mock<IAuthService> _authService = null!;
        private Mock<IUserSession> _session = null!;

        [SetUp]
        public void SetUp()
        {
            _authService = new Mock<IAuthService>();
            _session = new Mock<IUserSession>();
        }

        private MainViewModel CreateFor(string roleName)
        {
            _session.Setup(s => s.CurrentUser).Returns(new UserDTO { Login = "user", RoleName = roleName });
            _session.Setup(s => s.IsProductManager).Returns(roleName == Roles.ProductManager);
            return new MainViewModel(_authService.Object, _session.Object);
        }

        [Test]
        public void Manager_CanOpenManagerPage()
        {
            var viewModel = CreateFor(Roles.ProductManager);

            Assert.That(viewModel.OpenManagerCommand.CanExecute(null), Is.True);
        }

        [Test]
        public void Seller_CannotOpenManagerPage()
        {
            var viewModel = CreateFor("Seller");

            Assert.That(viewModel.OpenManagerCommand.CanExecute(null), Is.False);
            Assert.That(viewModel.AccessMessage, Does.Contain("немає доступу"));
        }


    }
}