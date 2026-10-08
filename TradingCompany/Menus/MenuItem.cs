using System;

namespace StoreConsole.Menus
{
    public class MenuItem
    {
        public string Title { get; }
        public Action Action { get; } //Action -  зберігається посилання на метод
        public bool IsSubmenu { get; }

        public MenuItem(string title, Action action, bool isSubmenu = false)
        {
            Title = title;
            Action = action;
            IsSubmenu = isSubmenu;
        }
    }
}