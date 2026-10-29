using Microsoft.Extensions.DependencyInjection;
using StoreViewModels.Main;
using System.Windows;

namespace StoreDB.WPF
{
    public partial class ManagerWindow : Window
    {
        public ManagerWindow(ManagerViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            viewModel.OpenCategoriesRequested += ViewModel_OpenCategoriesRequested;
            viewModel.OpenProductsRequested += ViewModel_OpenProductsRequested;
        }

        private void ViewModel_OpenCategoriesRequested(object? sender, EventArgs e)
        {
            var window = App.Services.GetRequiredService<CategoriesWindow>();
            window.Owner = this;
            window.ShowDialog();
        }

        private void ViewModel_OpenProductsRequested(object? sender, EventArgs e)
        {
            var window = App.Services.GetRequiredService<ProductsWindow>();
            window.Owner = this;
            window.ShowDialog();
        }
    }
}