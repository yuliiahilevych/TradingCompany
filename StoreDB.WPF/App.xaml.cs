using System.Windows;
using System.Windows.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StoreBL.Categories;
using StoreBL.Products;
using StoreBL.Users;
using StoreDAL.EF.DAL;
using StoreDAL.EF.Data;
using StoreDAL.EF.Interfaces;
using StoreDB.WPF.Services;
using StoreViewModels.Base;
using StoreViewModels.Categories;
using StoreViewModels.Login;
using StoreViewModels.Main;
using StoreViewModels.Products;

namespace StoreDB.WPF
{
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var connectionString = configuration.GetConnectionString("StoreDB");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                MessageBox.Show("У appsettings.json не задано ConnectionStrings:StoreDB.", "Помилка конфігурації",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
                return;
            }

            var services = new ServiceCollection();
            ConfigureServices(services, connectionString);

            Services = services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true });

            var loginWindow = Services.GetRequiredService<LoginWindow>();
            MainWindow = loginWindow;
            loginWindow.Show();
        }

        private static void ConfigureServices(IServiceCollection services, string connectionString)
        {
            services.AddDbContext<TradingCompanyContext>( // новий контекст для кожного DAL
                options => options.UseSqlServer(connectionString),
                ServiceLifetime.Transient,
                ServiceLifetime.Singleton);

            services.AddTransient<IUserDAL, UserDAL>();
            services.AddTransient<ICategoryDAL, CategoryDAL>();
            services.AddTransient<IProductDAL, ProductDAL>();
            services.AddTransient<IPriceHistoryDAL, PriceHistoryDAL>();

            // BL
            services.AddSingleton<IPasswordHasher, PasswordHasher>();
            services.AddSingleton<IUserSession, UserSession>();
            services.AddTransient<IAuthService, AuthService>();
            services.AddTransient<ICategoryService, CategoryService>();
            services.AddTransient<IProductService, ProductService>();

            services.AddSingleton<IDialogService, MessageBoxDialogService>();

            // ViewModel
            services.AddTransient<LoginViewModel>();
            services.AddTransient<MainViewModel>();
            services.AddTransient<ManagerViewModel>();
            services.AddTransient<CategoriesViewModel>();
            services.AddTransient<ProductsViewModel>();
            services.AddTransient<PriceHistoryViewModel>();

            // Вікна
            services.AddTransient<LoginWindow>();
            services.AddTransient<MainWindow>();
            services.AddTransient<ManagerWindow>();
            services.AddTransient<CategoriesWindow>();
            services.AddTransient<ProductsWindow>();
            services.AddTransient<PriceHistoryWindow>();

        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.Message, "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }
    }
}