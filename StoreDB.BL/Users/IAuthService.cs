namespace StoreBL.Users
{
    public interface IAuthService
    {
        bool Login(string login, string password);

        void Logout();
    }
}