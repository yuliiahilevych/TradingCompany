using Microsoft.EntityFrameworkCore;
using StoreDAL.EF.Entities;

namespace StoreDAL.EF.Data;

public partial class TradingCompanyContext : DbContext
{
    public TradingCompanyContext(DbContextOptions<TradingCompanyContext> options)
        : base(options)
    {
    }

    public virtual DbSet<tblCategory> tblCategory { get; set; }

    public virtual DbSet<tblProduct> tblProduct { get; set; }

    public virtual DbSet<tblPriceHistory> tblPriceHistory { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<tblCategory>(entity =>
        {
            entity.ToTable("tblCategory");

            entity.HasKey(e => e.CategoryId);

            entity.HasIndex(e => e.CategoryName).IsUnique();

            entity.Property(e => e.CategoryName).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(500);
        });

        modelBuilder.Entity<tblProduct>(entity =>
        {
            entity.ToTable("tblProduct");

            entity.HasKey(e => e.ProductId);

            entity.HasIndex(e => new { e.CategoryId, e.ProductName }).IsUnique();

            entity.Property(e => e.ProductName).HasMaxLength(50);
            entity.Property(e => e.PurchasePrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.SellingPrice).HasColumnType("decimal(10, 2)");

            // зв'язок : багато товарів → одна категорія
            entity.HasOne(p => p.Category)
                  .WithMany(c => c.Products)
                  .HasForeignKey(p => p.CategoryId)
                  .OnDelete(DeleteBehavior.ClientSetNull)
                  .HasConstraintName("FK_tblProduct_tblCategory");
        });

        modelBuilder.Entity<tblPriceHistory>(entity =>
        {
            entity.ToTable("tblPriceHistory");

            entity.HasKey(e => e.PriceHistoryId);

            entity.Property(e => e.OldPurchasePrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.NewPurchasePrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.OldSellingPrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.NewSellingPrice).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ChangedAt).HasDefaultValueSql("(sysdatetime())");

            // зв'язок: багато записів історії → один товар (видаляються разом із товаром)
            entity.HasOne(h => h.Product)
                  .WithMany(p => p.PriceHistories)
                  .HasForeignKey(h => h.ProductId)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("FK_tblPriceHistory_tblProduct");
        });
    }
    public override int SaveChanges()
    {
        foreach (var entry in ChangeTracker.Entries<tblProduct>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.RowInsertTime = DateTime.Now;
            else if (entry.State == EntityState.Modified)
                entry.Entity.RowUpdateTime = DateTime.Now;
        }
        return base.SaveChanges();
    }
}