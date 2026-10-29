using System.Collections.ObjectModel;
using System.ComponentModel;
using StoreBL.Categories;
using StoreBL.Exceptions;
using StoreBL.Products;
using StoreDTO;
using StoreViewModels.Base;

namespace StoreViewModels.Products
{
    public class ProductsViewModel : ViewModelBase
    {
        public const int AllCategoriesId = 0;

        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IDialogService _dialogService;

        private string _searchText = string.Empty;
        private int _filterCategoryId = AllCategoriesId;
        private ProductListItem? _selectedProduct;
        private ProductEditorViewModel? _editor;
        private string? _errorMessage;
        private string? _statusMessage;

        public ProductsViewModel(IProductService productService, ICategoryService categoryService,
                                 IDialogService dialogService)
        {
            _productService = productService;
            _categoryService = categoryService;
            _dialogService = dialogService;

            LoadCommand = new RelayCommand(Load);
            AddCommand = new RelayCommand(Add, () => ActiveCategories.Count > 0);
            SaveCommand = new RelayCommand(Save,
                () => Editor is { HasErrors: false } editor && (editor.IsNew || editor.IsDirty));
            CancelCommand = new RelayCommand(Cancel, () => Editor != null);
            BlockCommand = new RelayCommand(Block, () => SelectedProduct is { IsBlocked: false });
            UnblockCommand = new RelayCommand(Unblock, () => SelectedProduct is { IsBlocked: true });
            ShowHistoryCommand = new RelayCommand(
                () => ShowPriceHistoryRequested?.Invoke(this, EventArgs.Empty),
                () => SelectedProduct != null);
            DeleteCommand = new RelayCommand(Delete, () => SelectedProduct is { IsBlocked: true });
        }

        public ObservableCollection<ProductListItem> Products { get; } = new();
        public ObservableCollection<CategoryDTO> FilterCategories { get; } = new();
        public ObservableCollection<CategoryDTO> ActiveCategories { get; } = new();

        public string SearchText
        {
            get => _searchText;
            set { if (SetProperty(ref _searchText, value)) LoadProducts(null); }
        }

        public int FilterCategoryId
        {
            get => _filterCategoryId;
            set { if (SetProperty(ref _filterCategoryId, value)) LoadProducts(null); }
        }

        public ProductListItem? SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                if (SetProperty(ref _selectedProduct, value))
                {
                    Editor = value == null ? null : new ProductEditorViewModel(value.Product);
                    ErrorMessage = null;
                    RaiseCommandsChanged();
                }
            }
        }

        public ProductEditorViewModel? Editor
        {
            get => _editor;
            private set
            {
                if (_editor != null) _editor.PropertyChanged -= Editor_PropertyChanged;

                if (SetProperty(ref _editor, value))
                {
                    if (_editor != null) _editor.PropertyChanged += Editor_PropertyChanged;
                    OnPropertyChanged(nameof(IsEditing));
                    RaiseCommandsChanged();
                }
            }
        }

        public bool IsEditing => Editor != null;

        public string? ErrorMessage
        {
            get => _errorMessage;
            private set => SetProperty(ref _errorMessage, value);
        }

        public string? StatusMessage
        {
            get => _statusMessage;
            private set => SetProperty(ref _statusMessage, value);
        }

        public RelayCommand LoadCommand { get; }
        public RelayCommand AddCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand CancelCommand { get; }
        public RelayCommand BlockCommand { get; }
        public RelayCommand UnblockCommand { get; }
        public RelayCommand ShowHistoryCommand { get; }
        public RelayCommand DeleteCommand { get; }
        public event EventHandler? ShowPriceHistoryRequested;

        public void Load()
        {
            try
            {
                var categories = _categoryService.GetAll();

                FilterCategories.Clear();
                FilterCategories.Add(new CategoryDTO { CategoryId = AllCategoriesId, CategoryName = "Усі категорії" });
                foreach (var category in categories)
                    FilterCategories.Add(category);

                ActiveCategories.Clear();
                foreach (var category in categories.Where(c => !c.IsDeleted))
                    ActiveCategories.Add(category);

                AddCommand.RaiseCanExecuteChanged();
                LoadProducts(null);
            }
            catch (BusinessRuleException ex)
            {
                ShowError(ex.Message);
            }
        }

        private void LoadProducts(int? selectId)
        {
            try
            {
                var names = FilterCategories.ToDictionary(c => c.CategoryId, c => c.CategoryName);
                int? categoryId = FilterCategoryId == AllCategoriesId ? null : FilterCategoryId;

                Products.Clear();
                foreach (var product in _productService.Search(SearchText, categoryId))
                {
                    var categoryName = names.TryGetValue(product.CategoryId, out var name) ? name : "?";
                    Products.Add(new ProductListItem(product, categoryName));
                }

                SelectedProduct = selectId == null
                    ? null
                    : Products.FirstOrDefault(p => p.ProductId == selectId);
            }
            catch (BusinessRuleException ex)
            {
                ShowError(ex.Message);
            }
        }

        private void Add()
        {
            var categoryId = ActiveCategories.Any(c => c.CategoryId == FilterCategoryId)
                ? FilterCategoryId
                : ActiveCategories.First().CategoryId;

            SelectedProduct = null;
            Editor = new ProductEditorViewModel(new ProductDTO { CategoryId = categoryId });
            ClearMessages();
        }

        private void Save()
        {
            if (Editor == null) return;

            if (!Editor.ValidateAll())
            {
                ShowError("Виправте помилки у формі: наведіть мишку на поле з «!», щоб побачити підказку.");
                return;
            }

            try
            {
                var isNew = Editor.IsNew;
                var product = Editor.ToDto();
                var name = product.ProductName.Trim();
                int productId;

                if (isNew)
                {
                    productId = _productService.Create(product).ProductId;
                }
                else if (_productService.Update(product))
                {
                    productId = product.ProductId;
                }
                else
                {
                    LoadProducts(null);
                    ShowError("Товар не знайдено. Можливо, його вже видалили.");
                    return;
                }

                LoadProducts(productId);
                ShowSuccess(isNew ? $"Товар \"{name}\" додано." : $"Товар \"{name}\" змінено.");
            }
            catch (BusinessRuleException ex)
            {
                ShowError(ex.Message);
            }
        }

        private void Cancel()
        {
            Editor = SelectedProduct == null ? null : new ProductEditorViewModel(SelectedProduct.Product);
            ShowSuccess("Зміни скасовано.");
        }

        private void Block()
        {
            var product = SelectedProduct!;
            if (!_dialogService.Confirm($"Заблокувати товар \"{product.ProductName}\"?", "Підтвердження")) return;

            try
            {
                _productService.Block(product.ProductId);
                LoadProducts(product.ProductId);
                ShowSuccess($"Товар \"{product.ProductName}\" заблоковано.");
            }
            catch (BusinessRuleException ex)
            {
                ShowError(ex.Message);
            }
        }

        private void Unblock()
        {
            var product = SelectedProduct!;

            try
            {
                _productService.Unblock(product.ProductId);
                LoadProducts(product.ProductId);
                ShowSuccess($"Товар \"{product.ProductName}\" розблоковано.");
            }
            catch (BusinessRuleException ex)
            {
                ShowError(ex.Message);
            }
        }

        private void Delete()
        {
            var product = SelectedProduct!;
            var question = $"Видалити товар \"{product.ProductName}\" назавжди?\n" +
                           "Разом з ним буде видалено історію цін. Цю дію не можна скасувати.";

            if (!_dialogService.Confirm(question, "Видалення")) return;

            try
            {
                _productService.Delete(product.ProductId);
                LoadProducts(null);
                ShowSuccess($"Товар \"{product.ProductName}\" видалено.");
            }
            catch (BusinessRuleException ex)
            {
                ShowError(ex.Message);
            }
        }

        private void ShowSuccess(string message)
        {
            ErrorMessage = null;
            StatusMessage = message;
        }

        private void ShowError(string message)
        {
            StatusMessage = null;
            ErrorMessage = message;
        }

        private void ClearMessages()
        {
            ErrorMessage = null;
            StatusMessage = null;
        }

        private void Editor_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(ProductEditorViewModel.IsDirty) or nameof(ProductEditorViewModel.HasErrors))
            {
                SaveCommand.RaiseCanExecuteChanged();
            }
        }

        private void RaiseCommandsChanged()
        {
            SaveCommand.RaiseCanExecuteChanged();
            CancelCommand.RaiseCanExecuteChanged();
            BlockCommand.RaiseCanExecuteChanged();
            UnblockCommand.RaiseCanExecuteChanged();
            ShowHistoryCommand.RaiseCanExecuteChanged();
            DeleteCommand.RaiseCanExecuteChanged();
        }
    }
}