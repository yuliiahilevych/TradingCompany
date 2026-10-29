    using StoreBL.Users;
using StoreViewModels.Base;

namespace StoreViewModels.Main
{
    public class ManagerViewModel : ViewModelBase
    {
        private readonly IUserSession _userSession;

        public ManagerViewModel(IUserSession userSession)
        {
            _userSession = userSession;

            OpenCategoriesCommand = new RelayCommand(
                () => OpenCategoriesRequested?.Invoke(this, EventArgs.Empty),
                () => _userSession.IsProductManager);

            OpenProductsCommand = new RelayCommand(
                () => OpenProductsRequested?.Invoke(this, EventArgs.Empty),
                () => _userSession.IsProductManager);
        }

        public string UserName => _userSession.CurrentUser?.Login ?? string.Empty;

        public RelayCommand OpenCategoriesCommand { get; }
        public RelayCommand OpenProductsCommand { get; }

        public event EventHandler? OpenCategoriesRequested;
        public event EventHandler? OpenProductsRequested;
    }
}