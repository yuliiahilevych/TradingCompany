using System;
using System.Collections.Generic;

namespace StoreConsole.Menus
{
    public class Menu
    {
        private readonly List<MenuItem> _items = new();
        private readonly bool _isMainMenu;

        public string Title { get; }

        public static Action<Exception>? ErrorHandler { get; set; }

        public Menu(string title, bool isMainMenu = false)
        {
            Title = title;
            _isMainMenu = isMainMenu;
        }

        public Menu Add(string title, Action action)
        {
            _items.Add(new MenuItem(title, action));
            return this;
        }
        public Menu Add(Menu submenu)
        {
            _items.Add(new MenuItem(submenu.Title, submenu.Run, isSubmenu: true));
            return this;
        }

        public void Run()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine($" {Title}  ");
                for (int i = 0; i < _items.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {_items[i].Title}");
                }
                Console.WriteLine(_isMainMenu ? "0. Exit" : "0. Back");

                Console.Write("Your choice: ");
                if (!int.TryParse(Console.ReadLine(), out int choice) || choice < 0 || choice > _items.Count)
                {
                    continue;  
                }
                if (choice == 0) return;

                var item = _items[choice - 1];


                if (item.IsSubmenu)
                {
                    item.Action();
                    continue;
                }

                try
                {
                    item.Action();
                }
                catch (Exception ex)
                {
                    if (ErrorHandler != null) ErrorHandler(ex);
                    else Console.WriteLine("Error: " + ex.Message);
                }

                Console.WriteLine();
                Console.Write("Press Enter to continue...");
                Console.ReadLine();
            }
        }
    }
}