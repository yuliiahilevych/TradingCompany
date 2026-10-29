using NUnit.Framework;
using StoreBL.Users;
using StoreDTO;

namespace StoreBL.Tests
{
    [TestFixture]
    public class UserSessionTests
    {
        private UserSession _session = null!;

        [SetUp]
        public void SetUp()
        {
            _session = new UserSession();
        }

        [Test]
        public void SignIn_Manager_IsProductManager()
        {
            _session.SignIn(new UserDTO { Login = "manager", RoleName = Roles.ProductManager });

            Assert.That(_session.IsProductManager, Is.True);
        }

        [Test]
        public void SignIn_Seller_IsNotProductManager()
        {
            _session.SignIn(new UserDTO { Login = "seller", RoleName = "Seller" });

            Assert.That(_session.IsProductManager, Is.False);
        }

        [Test]
        public void SignOut_ClearsCurrentUser()
        {
            _session.SignIn(new UserDTO { Login = "manager", RoleName = Roles.ProductManager });

            _session.SignOut();

            Assert.That(_session.IsAuthenticated, Is.False);
        }
    }
}