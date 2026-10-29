using System.Collections.ObjectModel;
using StoreBL.Exceptions;
using StoreBL.Products;
using StoreDTO;
using StoreViewModels.Base;

namespace StoreViewModels.Products
{
    public class PriceHistoryViewModel : ViewModelBase
    {
        private readonly IProductService _productService;
        private string _productName = string.Empty;
        private string? _errorMessage;

        public PriceHistoryViewModel(IProductService productService)
        {
            _productService = productService;
        }

        public ObservableCollection<PriceHistoryDTO> History { get; } = new();

        public string ProductName
        {
            get => _productName;
            private set
            {
                if (SetProperty(ref _productName, value))
                    OnPropertyChanged(nameof(Title));
            }
        }

        public string Title => $"Історія цін: {ProductName}";

        public bool IsEmpty => History.Count == 0;

        public string? ErrorMessage
        {
            get => _errorMessage;
            private set => SetProperty(ref _errorMessage, value);
        }

        public void Load(int productId, string productName)
        {
            ProductName = productName;

            try
            {
                History.Clear();
                foreach (var record in _productService.GetPriceHistory(productId))
                    History.Add(record);
            }
            catch (BusinessRuleException ex)
            {
                ErrorMessage = ex.Message;
            }

            OnPropertyChanged(nameof(IsEmpty));
        }
    }
}