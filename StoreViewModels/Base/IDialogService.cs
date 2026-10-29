namespace StoreViewModels.Base
{
    public interface IDialogService
    {
        bool Confirm(string message, string title);
    }
}