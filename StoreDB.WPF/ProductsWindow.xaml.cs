using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using StoreViewModels.Products;

namespace StoreDB.WPF
{
    public partial class ProductsWindow : Window
    {
        private readonly ProductsViewModel _viewModel;

        public ProductsWindow(ProductsViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = viewModel;
            viewModel.ShowPriceHistoryRequested += ViewModel_ShowPriceHistoryRequested;
            viewModel.Load();
        }

        private void ViewModel_ShowPriceHistoryRequested(object? sender, EventArgs e)
        {
            var product = _viewModel.SelectedProduct;
            if (product == null) return;

            var window = App.Services.GetRequiredService<PriceHistoryWindow>();
            window.Owner = this;
            window.Load(product.ProductId, product.ProductName);
            window.ShowDialog();
        }

        private void MenuClose_Click(object sender, RoutedEventArgs e) => Close();
    }
}