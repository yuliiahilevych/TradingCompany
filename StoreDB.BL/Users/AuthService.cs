using StoreDAL.EF.Interfaces;

namespace StoreBL.Users
{
    public class AuthService : IAuthService
    {
        public const int LoginMaxLength = 50;
        private readonly IUserDAL _userDal;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IUserSession _userSession;

        public AuthService(IUserDAL userDal, IPasswordHasher passwordHasher, IUserSession userSession)
        {
            _userDal = userDal;
            _passwordHasher = passwordHasher;
            _userSession = userSession;
        }

        public bool Login(string login, string password)
        {
            login = login?.Trim() ?? string.Empty;
            if (login.Length == 0 || string.IsNullOrEmpty(password))
            {
                return false;
            }

            var user = _userDal.GetByLogin(login);
            if (user == null)
            {
                _passwordHasher.Hash(password, Guid.Empty);
                return false;
            }

            if (!_passwordHasher.Verify(password, user.PasswordSalt, user.PasswordHash))
            {
                return false;
            }

            if (!user.IsActive)
            {
                return false;
            }

            _userSession.SignIn(user);
            return true;
        }

        public void Logout()
        {
            _userSession.SignOut();
        }
    }
}