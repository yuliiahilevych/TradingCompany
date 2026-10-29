using StoreBL.Users;
using StoreViewModels.Base;

namespace StoreViewModels.Login
{
    public class LoginViewModel : ValidatableViewModelBase
    {
        private readonly IAuthService _authService;

        private string _login = string.Empty;
        private string? _errorMessage;

        public LoginViewModel(IAuthService authService)
        {
            _authService = authService;
            LoginCommand = new RelayCommand(SignIn);
        }

        public string Login
        {
            get => _login;
            set
            {
                if (SetProperty(ref _login, value))
                {
                    ValidateLogin();
                }
            }
        }
        public string Password { private get; set; } = string.Empty;

        public string? ErrorMessage
        {
            get => _errorMessage;
            private set => SetProperty(ref _errorMessage, value);
        }

        public RelayCommand LoginCommand { get; }

        public event EventHandler? LoginSucceeded;
        public event EventHandler? LoginFailed;

        private void SignIn()
        {
            if (!ValidateAll())
            {
                return;
            }

            if (Password.Length == 0)
            {
                ErrorMessage = "Введіть пароль.";
                LoginFailed?.Invoke(this, EventArgs.Empty);
                return;
            }

            var success = _authService.Login(Login, Password);
            Password = string.Empty;

            if (!success)
            {
                ErrorMessage = "Невірний логін або пароль.";
                LoginFailed?.Invoke(this, EventArgs.Empty);
                return;
            }

            ErrorMessage = null;
            LoginSucceeded?.Invoke(this, EventArgs.Empty);
        }

        protected override void ValidateAllProperties()
        {
            ValidateLogin();
        }

        private void ValidateLogin()
        {
            var errors = new List<string>();
            var login = Login.Trim();

            if (login.Length == 0)
                errors.Add("Введіть логін.");
            else if (login.Length > AuthService.LoginMaxLength)
                errors.Add($"Логін не довший за {AuthService.LoginMaxLength} символів.");

            SetErrors(nameof(Login), errors);
        }
    }
}