using NUnit.Framework;
using StoreBL.Users;

namespace StoreBL.Tests
{
    [TestFixture]
    public class PasswordHasherTests
    {
        private PasswordHasher _hasher = null!;

        [SetUp]
        public void SetUp()
        {
            _hasher = new PasswordHasher();
        }

        [Test]
        public void Hash_SamePasswordDifferentSalt_GivesDifferentHash()
        {
            var first = _hasher.Hash("manager123", Guid.NewGuid());
            var second = _hasher.Hash("manager123", Guid.NewGuid());

            Assert.That(first, Is.Not.EqualTo(second));
        }

        [Test]
        public void Verify_CorrectPassword_ReturnsTrue()
        {
            var salt = Guid.NewGuid();
            var hash = _hasher.Hash("manager123", salt);

            Assert.That(_hasher.Verify("manager123", salt, hash), Is.True);
        }

        [Test]
        public void Verify_WrongPassword_ReturnsFalse()
        {
            var salt = Guid.NewGuid();
            var hash = _hasher.Hash("manager123", salt);

            Assert.That(_hasher.Verify("wrong", salt, hash), Is.False);
        }

        [Test]
        public void Verify_UserFromDatabaseScript_ReturnsTrue()
        {
            var salt = Guid.Parse("B8C8729B-A1F8-4C28-BA21-AC2AB923DF85");
            var hash = Convert.FromHexString("4D36433DBC0FE7FD39216E5F0FD46B96BAA69B8C32EAA5EA6DB833545FCE883E");

            Assert.That(_hasher.Verify("manager123", salt, hash), Is.True);
        }
    }
}