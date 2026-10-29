using StoreDTO;

namespace StoreBL.Users
{
    public interface IUserSession
    {
        UserDTO? CurrentUser { get; }
        bool IsAuthenticated { get; }
        bool IsProductManager { get; }

        void SignIn(UserDTO user);
        void SignOut();
    }
}