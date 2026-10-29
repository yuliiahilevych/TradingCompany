using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using StoreViewModels.Main;

namespace StoreDB.WPF
{
    public partial class MainWindow : Window
    {
        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            viewModel.OpenManagerRequested += ViewModel_OpenManagerRequested;
            viewModel.LoggedOut += ViewModel_LoggedOut;
        }

        private void ViewModel_OpenManagerRequested(object? sender, EventArgs e)
        {
            var window = App.Services.GetRequiredService<ManagerWindow>();
            window.Owner = this;
            window.ShowDialog();
        }

        private void ViewModel_LoggedOut(object? sender, EventArgs e)
        {
            var loginWindow = App.Services.GetRequiredService<LoginWindow>();
            Application.Current.MainWindow = loginWindow;
            loginWindow.Show();
            Close();
        }
    }
}