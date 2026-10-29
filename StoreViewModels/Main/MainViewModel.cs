using StoreBL.Users;
using StoreViewModels.Base;

namespace StoreViewModels.Main
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IAuthService _authService;
        private readonly IUserSession _userSession;

        public MainViewModel(IAuthService authService, IUserSession userSession)
        {
            _authService = authService;
            _userSession = userSession;

            OpenManagerCommand = new RelayCommand(
                () => OpenManagerRequested?.Invoke(this, EventArgs.Empty),
                () => IsProductManager);

            LogoutCommand = new RelayCommand(Logout);
        }

        public string UserName => _userSession.CurrentUser?.Login ?? string.Empty;
        public string RoleName => _userSession.CurrentUser?.RoleName ?? string.Empty;
        public bool IsProductManager => _userSession.IsProductManager;

        public string AccessMessage => IsProductManager
            ? "Відкрийте сторінку своєї ролі через меню «Менеджер товарів»."
            : "У вас немає доступу до розділу менеджера товарів.";

        public RelayCommand OpenManagerCommand { get; }
        public RelayCommand LogoutCommand { get; }

        public event EventHandler? OpenManagerRequested;
        public event EventHandler? LoggedOut;

        private void Logout()
        {
            _authService.Logout();
            LoggedOut?.Invoke(this, EventArgs.Empty);
        }
    }
}