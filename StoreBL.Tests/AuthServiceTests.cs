using Moq;
using NUnit.Framework;
using StoreBL.Users;
using StoreDAL.EF.Interfaces;
using StoreDTO;

namespace StoreBL.Tests
{
    [TestFixture]
    public class AuthServiceTests
    {
        private Mock<IUserDAL> _userDal = null!;
        private Mock<IPasswordHasher> _hasher = null!;
        private Mock<IUserSession> _session = null!;
        private AuthService _service = null!;

        private UserDTO _manager = null!;

        [SetUp]
        public void SetUp()
        {
            _userDal = new Mock<IUserDAL>();
            _hasher = new Mock<IPasswordHasher>();
            _session = new Mock<IUserSession>();

            _manager = new UserDTO
            {
                UserId = 1,
                Login = "manager",
                PasswordSalt = Guid.NewGuid(),
                PasswordHash = new byte[32],
                RoleName = Roles.ProductManager,
                IsActive = true
            };
            _userDal.Setup(d => d.GetByLogin("manager")).Returns(_manager);
            _hasher.Setup(h => h.Verify("manager123", _manager.PasswordSalt, _manager.PasswordHash)).Returns(true);

            _service = new AuthService(_userDal.Object, _hasher.Object, _session.Object);
        }

        [Test]
        public void Login_CorrectPassword_SignsInAndReturnsTrue()
        {
            var result = _service.Login("manager", "manager123");

            Assert.That(result, Is.True);
            _session.Verify(s => s.SignIn(_manager), Times.Once);
        }

        [Test]
        public void Login_TrimsLoginBeforeSearch()
        {
            var result = _service.Login("  manager  ", "manager123");

            Assert.That(result, Is.True);
            _userDal.Verify(d => d.GetByLogin("manager"), Times.Once);  
        }

        [Test]
        public void Login_WrongPassword_ReturnsFalseAndDoesNotSignIn()
        {
            var result = _service.Login("manager", "wrong");

            Assert.That(result, Is.False);
            _session.Verify(s => s.SignIn(It.IsAny<UserDTO>()), Times.Never);
        }

        [Test]
        public void Login_UnknownLogin_ReturnsFalseButStillHashes()
        {
            var result = _service.Login("nobody", "manager123");

            Assert.That(result, Is.False);
            _hasher.Verify(h => h.Hash("manager123", Guid.Empty), Times.Once);
            _session.Verify(s => s.SignIn(It.IsAny<UserDTO>()), Times.Never);
        }

        [Test]
        public void Login_InactiveUser_ReturnsFalse()
        {
            _manager.IsActive = false;

            var result = _service.Login("manager", "manager123");

            Assert.That(result, Is.False);
            _session.Verify(s => s.SignIn(It.IsAny<UserDTO>()), Times.Never);
        }

        [TestCase("", "manager123")]
        [TestCase("   ", "manager123")]
        [TestCase("manager", "")]
        public void Login_EmptyLoginOrPassword_ReturnsFalseAndDoesNotCallDal(string login, string password)
        {
            var result = _service.Login(login, password);

            Assert.That(result, Is.False);
            _userDal.Verify(d => d.GetByLogin(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void Logout_SignsOut()
        {
            _service.Logout();

            _session.Verify(s => s.SignOut(), Times.Once);
        }
    }
}