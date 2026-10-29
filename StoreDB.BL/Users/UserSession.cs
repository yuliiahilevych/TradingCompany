using StoreDTO;

namespace StoreBL.Users
{
    public class UserSession : IUserSession
    {
        public UserDTO? CurrentUser { get; private set; }

        public bool IsAuthenticated => CurrentUser != null;

        public bool IsProductManager => CurrentUser?.RoleName == Roles.ProductManager;

        public void SignIn(UserDTO user)
        {
            CurrentUser = user;
        }

        public void SignOut()
        {
            CurrentUser = null;
        }
    }
}