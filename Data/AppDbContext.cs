using Microsoft.EntityFrameworkCore;
using POS.Models;

namespace POS.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();

    public DbSet<Sale> Sales => Set<Sale>();

    public DbSet<SaleItem> SaleItems => Set<SaleItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Products");

            entity.HasKey(p => p.Id);

            entity.Property(p => p.Barcode)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(p => p.Name)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(p => p.Category)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(p => p.ImageUrl)
                .HasMaxLength(500);

            entity.Property(p => p.Price)
                .HasColumnType("decimal(18,2)");

            entity.HasIndex(p => p.Barcode)
                .IsUnique();
        });

        modelBuilder.Entity<Sale>(entity =>
        {
            entity.ToTable("Sales");

            entity.HasKey(s => s.Id);

            entity.Property(s => s.Subtotal).HasColumnType("decimal(18,2)");
            entity.Property(s => s.Tax).HasColumnType("decimal(18,2)");
            entity.Property(s => s.Total).HasColumnType("decimal(18,2)");
            entity.Property(s => s.AmountPaid).HasColumnType("decimal(18,2)");
            entity.Property(s => s.ChangeDue).HasColumnType("decimal(18,2)");

            entity.HasMany(s => s.Items)
                .WithOne(i => i.Sale)
                .HasForeignKey(i => i.SaleId);
        });

        modelBuilder.Entity<SaleItem>(entity =>
        {
            entity.ToTable("SaleItems");

            entity.HasKey(i => i.Id);

            entity.Property(i => i.Barcode)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(i => i.ProductName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(i => i.Category)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(i => i.ImageUrl)
                .HasMaxLength(500);

            entity.Property(i => i.UnitPrice).HasColumnType("decimal(18,2)");
            entity.Property(i => i.LineTotal).HasColumnType("decimal(18,2)");

            entity.HasOne(i => i.Product)
                .WithMany()
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
