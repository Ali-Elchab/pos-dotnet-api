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

            entity.Property(p => p.Price)
                .HasColumnType("decimal(18,2)");

            entity.HasIndex(p => p.Barcode)
                .IsUnique();
        });
    }
}