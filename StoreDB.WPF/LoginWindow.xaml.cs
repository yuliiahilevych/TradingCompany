using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using StoreViewModels.Login;

namespace StoreDB.WPF
{
    public partial class LoginWindow : Window
    {
        private readonly LoginViewModel _viewModel;

        public LoginWindow(LoginViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = viewModel;

            viewModel.LoginSucceeded += ViewModel_LoginSucceeded;
            viewModel.LoginFailed += ViewModel_LoginFailed;
        }
        private void PwdPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            _viewModel.Password = pwdPassword.Password;
        }
        private void ViewModel_LoginSucceeded(object? sender, EventArgs e)
        {
            var mainWindow = App.Services.GetRequiredService<MainWindow>();

            Application.Current.MainWindow = mainWindow;
            mainWindow.Show();
            Close();
        }
        private void ViewModel_LoginFailed(object? sender, EventArgs e)
        {
            pwdPassword.Clear();
            pwdPassword.Focus();
        }
    }
}