using Moq;
using NUnit.Framework;
using StoreBL.Users;
using StoreViewModels.Login;

namespace StoreViewModels.Tests
{
    [TestFixture]
    public class LoginViewModelTests
    {
        private Mock<IAuthService> _authService = null!;
        private LoginViewModel _viewModel = null!;

        [SetUp]
        public void SetUp()
        {
            _authService = new Mock<IAuthService>();
            _authService.Setup(a => a.Login("manager", "manager123")).Returns(true);

            _viewModel = new LoginViewModel(_authService.Object);
        }

        private List<string> ErrorsOf(string propertyName) =>
            _viewModel.GetErrors(propertyName).Cast<string>().ToList();

        [Test]
        public void EmptyLogin_ShowsFieldError()
        {
            _viewModel.Login = "manager";
            _viewModel.Login = "";

            Assert.That(ErrorsOf(nameof(LoginViewModel.Login)), Is.EqualTo(new[] { "Введіть логін." }));
            Assert.That(_viewModel.HasErrors, Is.True);
        }

        [Test]
        public void LoginCommand_EmptyPassword_ShowsMessageWithoutCallingService()
        {
            _viewModel.Login = "manager";

            _viewModel.LoginCommand.Execute(null);

            Assert.That(_viewModel.ErrorMessage, Is.EqualTo("Введіть пароль."));
            _authService.Verify(a => a.Login(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void LoginCommand_WrongPassword_ShowsErrorAndRaisesLoginFailed()
        {
            var failed = false;
            _viewModel.LoginFailed += (_, _) => failed = true;
            _viewModel.Login = "manager";
            _viewModel.Password = "wrong";

            _viewModel.LoginCommand.Execute(null);

            Assert.That(_viewModel.ErrorMessage, Is.EqualTo("Невірний логін або пароль."));
            Assert.That(failed, Is.True);
        }

        [Test]
        public void LoginCommand_CorrectPassword_RaisesLoginSucceeded()
        {
            var succeeded = false;
            _viewModel.LoginSucceeded += (_, _) => succeeded = true;
            _viewModel.Login = "manager";
            _viewModel.Password = "manager123";

            _viewModel.LoginCommand.Execute(null);

            Assert.That(succeeded, Is.True);
            Assert.That(_viewModel.ErrorMessage, Is.Null);
        }
    }
}