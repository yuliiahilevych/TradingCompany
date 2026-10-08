using System;
using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using StoreDAL.EF.DAL;
using StoreDAL.EF.Data;
using StoreDAL.Interfaces;
using StoreDTO;
using StoreConsole.Menus;

namespace StoreConsole
{
    internal class Program
    {
        private static TradingCompanyContext _context = null!;
        private static ICategoryDAL _categoryDal = null!;
        private static IProductDAL _productDal = null!;
        private static IPriceHistoryDAL _historyDal = null!;

        static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.Unicode;

            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            string connectionString = config.GetConnectionString("StoreDB")
                ?? throw new InvalidOperationException("Connection string 'StoreDB' not found in appsettings.json");

            var options = new DbContextOptionsBuilder<TradingCompanyContext>()
                .UseSqlServer(connectionString)
                .Options;

            using (_context = new TradingCompanyContext(options))
            {
                if (!_context.Database.CanConnect())
                {
                    Console.WriteLine("Could not connect to the database. Check the connection string.");
                    return;
                }

                _categoryDal = new CategoryDAL(_context);
                _productDal = new ProductDAL(_context);
                _historyDal = new PriceHistoryDAL(_context);

                // якщо база повернула помилку: скасовуємо невдалі зміни й показуємо повідомлення
                Menu.ErrorHandler = ex =>
                {
                    _context.ChangeTracker.Clear();
                    if (ex is DbUpdateException)
                        Console.WriteLine("Database error: " + (ex.InnerException?.Message ?? ex.Message));
                    else
                        Console.WriteLine("Error: " + ex.Message);
                };

  
                var categoryMenu = new Menu("Categories")
                    .Add("Show all  ", () =>
                    {
                        foreach (var c in _categoryDal.GetAll()) PrintCategory(c);
                    })
                    .Add("Find by ID  ", () =>
                    {
                        var c = _categoryDal.GetById(ReadInt("Category ID"));
                        if (c == null) Console.WriteLine("Category not found.");
                        else PrintCategory(c);
                    })
                    .Add("Add  ", () =>
                    {
                        var created = _categoryDal.Create(new CategoryDTO
                        {
                            CategoryName = ReadString("Name"),
                            Description = ReadOptional("Description (Enter to skip)")
                        });
                        Console.WriteLine("Added:");
                        PrintCategory(created);
                    })
                    .Add("Update  ", () =>
                    {
                        var c = _categoryDal.GetById(ReadInt("Category ID"));
                        if (c == null) { Console.WriteLine("Category not found."); return; }

                        Console.WriteLine("Press Enter to keep the current value.");
                        c.CategoryName = ReadString("Name", c.CategoryName);
                        c.Description = ReadOptional("Description", c.Description);
                        c.IsDeleted = ReadBool("Mark as deleted? (yes/no)", c.IsDeleted);

                        Console.WriteLine(_categoryDal.Update(c) ? "Updated." : "Update failed.");
                    })
                    .Add("Delete  ", () =>
                    {
                        Console.WriteLine(_categoryDal.Delete(ReadInt("Category ID"))
                            ? "Deleted." : "Category not found.");
                    });

                var productMenu = new Menu("Products")
                    .Add("Show all  ", () =>
                    {
                        foreach (var p in _productDal.GetAll()) PrintProduct(p);
                    })
                    .Add("Find by ID  ", () =>
                    {
                        var p = _productDal.GetById(ReadInt("Product ID"));
                        if (p == null) Console.WriteLine("Product not found.");
                        else PrintProduct(p);
                    })
                    .Add("Add  ", () =>
                    {
                        var created = _productDal.Create(new ProductDTO
                        {
                            ProductName = ReadString("Name"),
                            CategoryId = ReadInt("Category ID"),
                            PurchasePrice = ReadDecimal("Purchase price"),
                            SellingPrice = ReadDecimal("Selling price")
                        });
                        Console.WriteLine("Added:");
                        PrintProduct(created);
                    })
                    .Add("Update  ", () =>
                    {
                        var p = _productDal.GetById(ReadInt("Product ID"));
                        if (p == null) { Console.WriteLine("Product not found."); return; }

                        Console.WriteLine("Press Enter to keep the current value.");
                        p.ProductName = ReadString("Name", p.ProductName);
                        p.CategoryId = ReadInt("Category ID", p.CategoryId);
                        p.PurchasePrice = ReadDecimal("Purchase price", p.PurchasePrice);
                        p.SellingPrice = ReadDecimal("Selling price", p.SellingPrice);
                        p.IsBlocked = ReadBool("Blocked? (yes/no)", p.IsBlocked);

                        Console.WriteLine(_productDal.Update(p) ? "Updated." : "Update failed.");
                    })
                    .Add("Delete  ", () =>
                    {
                        Console.WriteLine(_productDal.Delete(ReadInt("Product ID"))
                            ? "Deleted." : "Product not found.");
                    });

                var historyMenu = new Menu("Price history")
                    .Add("Show all  ", () =>
                    {
                        foreach (var h in _historyDal.GetAll()) PrintHistory(h);
                    })
                    .Add("Find by ID  ", () =>
                    {
                        var h = _historyDal.GetById(ReadInt("Record ID"));
                        if (h == null) Console.WriteLine("Record not found.");
                        else PrintHistory(h);
                    })
                    .Add("Add  ", () =>
                    {
                        var product = _productDal.GetById(ReadInt("Product ID"));
                        if (product == null) { Console.WriteLine("Product not found."); return; }

                        Console.WriteLine($"Product: {product.ProductName}");
                        Console.WriteLine("Old prices = current product prices (press Enter to keep them).");

                        var created = _historyDal.Create(new PriceHistoryDTO
                        {
                            ProductId = product.ProductId,
                            OldPurchasePrice = ReadDecimal("Old purchase price", product.PurchasePrice),
                            NewPurchasePrice = ReadDecimal("New purchase price"),
                            OldSellingPrice = ReadDecimal("Old selling price", product.SellingPrice),
                            NewSellingPrice = ReadDecimal("New selling price")
                        });
                        Console.WriteLine("Added:");
                        PrintHistory(created);
                    })
                    .Add("Update  ", () =>
                    {
                        var h = _historyDal.GetById(ReadInt("Record ID"));
                        if (h == null) { Console.WriteLine("Record not found."); return; }

                        Console.WriteLine("Press Enter to keep the current value.");
                        h.OldPurchasePrice = ReadDecimal("Old purchase price", h.OldPurchasePrice);
                        h.NewPurchasePrice = ReadDecimal("New purchase price", h.NewPurchasePrice);
                        h.OldSellingPrice = ReadDecimal("Old selling price", h.OldSellingPrice);
                        h.NewSellingPrice = ReadDecimal("New selling price", h.NewSellingPrice);

                        Console.WriteLine(_historyDal.Update(h) ? "Updated." : "Update failed.");
                    })
                    .Add("Delete  ", () =>
                    {
                        Console.WriteLine(_historyDal.Delete(ReadInt("Record ID"))
                            ? "Deleted." : "Record not found.");
                    });

                new Menu("Trading Company", isMainMenu: true)
                    .Add(categoryMenu)
                    .Add(productMenu)
                    .Add(historyMenu)
                    .Run();
            }
        }

        static void PrintCategory(CategoryDTO c) =>
            Console.WriteLine($"{c.CategoryId,4} | {c.CategoryName,-20} | {(c.IsDeleted ? "deleted" : "active"),-9} | {c.Description}");

        static void PrintProduct(ProductDTO p) =>
            Console.WriteLine($"{p.ProductId,4} | {p.ProductName,-30} | cat. {p.CategoryId,3} | " +
                              $"buy {p.PurchasePrice,10:0.00} | sell {p.SellingPrice,10:0.00} | " +
                              $"{(p.IsBlocked ? "BLOCKED" : "")}");

        static void PrintHistory(PriceHistoryDTO h)
        {
            var product = _productDal.GetById(h.ProductId);
            string productName = product?.ProductName ?? "(product not found)";

            Console.WriteLine($"#{h.PriceHistoryId} | {h.ChangedAt:dd.MM.yyyy HH:mm} | Product {h.ProductId}: {productName}");
            Console.WriteLine($"     Purchase price: was {h.OldPurchasePrice:0.00} -> now {h.NewPurchasePrice:0.00}");
            Console.WriteLine($"     Selling price:  was {h.OldSellingPrice:0.00} -> now {h.NewSellingPrice:0.00}");
            Console.WriteLine();
        }

        static string ReadString(string prompt, string? current = null)
        {
            while (true)
            {
                Console.Write(current == null ? $"{prompt}: " : $"{prompt} [{current}]: ");
                string? input = Console.ReadLine()?.Trim();
                if (!string.IsNullOrEmpty(input)) return input;
                if (current != null) return current;
                Console.WriteLine("  Value cannot be empty.");
            }
        }

        static string? ReadOptional(string prompt, string? current = null)
        {
            Console.Write(current == null ? $"{prompt}: " : $"{prompt} [{current}]: ");
            string? input = Console.ReadLine()?.Trim();
            return string.IsNullOrEmpty(input) ? current : input;
        }

        static int ReadInt(string prompt, int? current = null)
        {
            while (true)
            {
                Console.Write(current == null ? $"{prompt}: " : $"{prompt} [{current}]: ");
                string? input = Console.ReadLine()?.Trim();
                if (string.IsNullOrEmpty(input) && current != null) return current.Value;
                if (int.TryParse(input, out int value)) return value;
                Console.WriteLine("  Enter a whole number.");
            }
        }

        static decimal ReadDecimal(string prompt, decimal? current = null)
        {
            while (true)
            {
                Console.Write(current == null ? $"{prompt}: " : $"{prompt} [{current:0.00}]: ");
                string? input = Console.ReadLine()?.Trim().Replace(',', '.');
                if (string.IsNullOrEmpty(input) && current != null) return current.Value;
                if (decimal.TryParse(input, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal value) && value >= 0)
                    return value;
                Console.WriteLine("  Enter a number >= 0 (e.g. 199.99).");
            }
        }

        static bool ReadBool(string prompt, bool current)
        {
            while (true)
            {
                Console.Write($"{prompt} [{(current ? "yes" : "no")}]: ");
                string? input = Console.ReadLine()?.Trim().ToLower();
                if (string.IsNullOrEmpty(input)) return current;
                if (input is "yes" or "y" or "1" or "так") return true;
                if (input is "no" or "n" or "0" or "ні") return false;
                Console.WriteLine("  Enter yes or no.");
            }
        }
    }
}