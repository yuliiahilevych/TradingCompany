using System.Windows;
using StoreViewModels.Categories;

namespace StoreDB.WPF
{
    public partial class CategoriesWindow : Window
    {
        public CategoriesWindow(CategoriesViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            viewModel.Load();
        }
        private void MenuClose_Click(object sender, RoutedEventArgs e) => Close();
    }
}