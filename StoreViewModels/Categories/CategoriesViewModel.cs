using System.Collections.ObjectModel;
using System.ComponentModel;
using StoreBL.Categories;
using StoreBL.Exceptions;
using StoreDTO;
using StoreViewModels.Base;

namespace StoreViewModels.Categories
{
    public class CategoriesViewModel : ViewModelBase
    {
        private readonly ICategoryService _categoryService;
        private readonly IDialogService _dialogService;

        private CategoryDTO? _selectedCategory;
        private CategoryEditorViewModel? _editor;
        private string? _errorMessage;
        private string? _statusMessage;

        public CategoriesViewModel(ICategoryService categoryService, IDialogService dialogService)
        {
            _categoryService = categoryService;
            _dialogService = dialogService;

            LoadCommand = new RelayCommand(() => Load());
            AddCommand = new RelayCommand(Add);
            SaveCommand = new RelayCommand(Save,
                () => Editor is { HasErrors: false } editor && (editor.IsNew || editor.IsDirty));
            CancelCommand = new RelayCommand(Cancel, () => Editor != null);
            MarkDeletedCommand = new RelayCommand(MarkDeleted, () => SelectedCategory is { IsDeleted: false });
            RestoreCommand = new RelayCommand(Restore, () => SelectedCategory is { IsDeleted: true });
            DeleteCommand = new RelayCommand(Delete, () => SelectedCategory != null);
        }

        public ObservableCollection<CategoryDTO> Categories { get; } = new();

        public CategoryDTO? SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (SetProperty(ref _selectedCategory, value))
                {
                    Editor = value == null ? null : new CategoryEditorViewModel(value);
                    ErrorMessage = null;
                    RaiseCommandsChanged();
                }
            }
        }

        public CategoryEditorViewModel? Editor
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
        public RelayCommand MarkDeletedCommand { get; }
        public RelayCommand RestoreCommand { get; }
        public RelayCommand DeleteCommand { get; }

        public void Load(int? selectId = null)
        {
            try
            {
                Categories.Clear();
                foreach (var category in _categoryService.GetAll())
                    Categories.Add(category);

                SelectedCategory = selectId == null
                    ? null
                    : Categories.FirstOrDefault(c => c.CategoryId == selectId);
            }
            catch (BusinessRuleException ex)
            {
                ShowError(ex.Message);
            }
        }

        private void Add()
        {
            SelectedCategory = null;
            Editor = new CategoryEditorViewModel(new CategoryDTO());
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
                var category = Editor.ToDto();
                var name = category.CategoryName.Trim();
                int categoryId;

                if (isNew)
                {
                    categoryId = _categoryService.Create(category).CategoryId;
                }
                else if (_categoryService.Update(category))
                {
                    categoryId = category.CategoryId;
                }
                else
                {
                    Load();
                    ShowError("Категорію не знайдено. Можливо, її вже видалили.");
                    return;
                }

                Load(categoryId);
                ShowSuccess(isNew ? $"Категорію \"{name}\" додано." : $"Категорію \"{name}\" змінено.");
            }
            catch (BusinessRuleException ex)
            {
                ShowError(ex.Message);
            }
        }

        private void Cancel()
        {
            Editor = SelectedCategory == null ? null : new CategoryEditorViewModel(SelectedCategory);
            ShowSuccess("Зміни скасовано.");
        }

        private void MarkDeleted()
        {
            var category = SelectedCategory!;
            var question = $"Позначити категорію \"{category.CategoryName}\" на видалення?\n" +
                           "Усі її товари буде заблоковано.";

            if (!_dialogService.Confirm(question, "Підтвердження")) return;

            try
            {
                _categoryService.MarkDeleted(category.CategoryId);
                Load(category.CategoryId);
                ShowSuccess($"Категорію \"{category.CategoryName}\" позначено на видалення, її товари заблоковано.");
            }
            catch (BusinessRuleException ex)
            {
                ShowError(ex.Message);
            }
        }

        private void Restore()
        {
            var category = SelectedCategory!;

            try
            {
                _categoryService.Restore(category.CategoryId);
                Load(category.CategoryId);
                ShowSuccess($"Категорію \"{category.CategoryName}\" відновлено. Товари лишились заблокованими — розблокуйте потрібні вручну.");
            }
            catch (BusinessRuleException ex)
            {
                ShowError(ex.Message);
            }
        }

        private void Delete()
        {
            var category = SelectedCategory!;
            var question = $"Видалити категорію \"{category.CategoryName}\" назавжди?\n" +
                           "Цю дію не можна скасувати.";

            if (!_dialogService.Confirm(question, "Видалення")) return;

            try
            {
                _categoryService.Delete(category.CategoryId);
                Load();
                ShowSuccess($"Категорію \"{category.CategoryName}\" видалено.");
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
            if (e.PropertyName is nameof(CategoryEditorViewModel.IsDirty) or nameof(CategoryEditorViewModel.HasErrors))
            {
                SaveCommand.RaiseCanExecuteChanged();
            }
        }

        private void RaiseCommandsChanged()
        {
            SaveCommand.RaiseCanExecuteChanged();
            CancelCommand.RaiseCanExecuteChanged();
            MarkDeletedCommand.RaiseCanExecuteChanged();
            RestoreCommand.RaiseCanExecuteChanged();
            DeleteCommand.RaiseCanExecuteChanged();
        }
    }
}
