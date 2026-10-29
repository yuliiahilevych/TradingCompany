using System.Windows;
using StoreViewModels.Products;

namespace StoreDB.WPF
{
    public partial class PriceHistoryWindow : Window
    {
        private readonly PriceHistoryViewModel _viewModel;

        public PriceHistoryWindow(PriceHistoryViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = viewModel;
        }

        public void Load(int productId, string productName) => _viewModel.Load(productId, productName);
    }
}