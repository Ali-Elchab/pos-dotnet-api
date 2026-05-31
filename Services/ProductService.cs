using Microsoft.EntityFrameworkCore;
using POS.Data;
using POS.DTOs.Products;
using POS.Models;
using System.ComponentModel.DataAnnotations;

namespace POS.Services.Products;

public class ProductService : IProductService
{
    private readonly AppDbContext _context;

    public ProductService(AppDbContext context) => _context = context;

    public async Task<List<ProductResponse>> GetAllAsync(string? barcode = null)
    {
        var query = _context.Products
            .AsNoTracking()
            .Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(barcode))
            query = query.Where(p => p.Barcode == barcode);

        return await query
            .Select(p => new ProductResponse
            {
                Id = p.Id,
                Barcode = p.Barcode,
                Name = p.Name,
                Category = p.Category,
                ImageUrl = p.ImageUrl,
                Price = p.Price,
                StockQuantity = p.StockQuantity
            })
            .ToListAsync();
    }

    public async Task<ProductResponse?> GetByIdAsync(int id)
    {
        var p = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive);

        return p is null ? null : Map(p);
    }

    public async Task<ProductResponse?> GetByBarcodeAsync(string barcode)
    {
        var p = await _context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Barcode == barcode && x.IsActive);

        return p is null ? null : Map(p);
    }

    public async Task<ProductResponse> CreateAsync(ProductCreateRequest request)
    {
        Validate(request.Barcode, request.Name, request.Category, request.Price, request.StockQuantity);

        var exists = await _context.Products
            .AnyAsync(x => x.Barcode == request.Barcode);
        if (exists) throw new ValidationException("Barcode must be unique");

        var p = new Product
        {
            Barcode = request.Barcode.Trim(),
            Name = request.Name.Trim(),
            Category = request.Category.Trim(),
            ImageUrl = NormalizeOptionalText(request.ImageUrl),
            Price = request.Price,
            StockQuantity = request.StockQuantity,
            IsActive = true
        };

        _context.Products.Add(p);
        await _context.SaveChangesAsync();

        return Map(p);
    }

    public async Task<ProductResponse?> UpdateAsync(int id, ProductUpdateRequest request)
    {
        Validate(request.Barcode, request.Name, request.Category, request.Price, request.StockQuantity);

        var p = await _context.Products.FirstOrDefaultAsync(x => x.Id == id && x.IsActive);
        if (p is null) return null;

        if (!string.Equals(p.Barcode, request.Barcode, StringComparison.Ordinal))
        {
            var conflict = await _context.Products.AnyAsync(x => x.Barcode == request.Barcode && x.Id != id);
            if (conflict) throw new ValidationException("Barcode must be unique");
        }

        p.Barcode = request.Barcode.Trim();
        p.Name = request.Name.Trim();
        p.Category = request.Category.Trim();
        p.ImageUrl = NormalizeOptionalText(request.ImageUrl);
        p.Price = request.Price;
        p.StockQuantity = request.StockQuantity;

        await _context.SaveChangesAsync();

        return Map(p);
    }

    // Deactivate keeps the entity but marks it inactive
    public async Task<bool> SoftDeleteAsync(int id) => await DeactivateAsync(id);

    public async Task<bool> DeactivateAsync(int id)
    {
        var p = await _context.Products.FirstOrDefaultAsync(x => x.Id == id && x.IsActive);
        if (p is null) return false;

        p.IsActive = false;
        await _context.SaveChangesAsync();
        return true;
    }

    private static ProductResponse Map(Product p) => new()
    {
        Id = p.Id,
        Barcode = p.Barcode,
        Name = p.Name,
        Category = p.Category,
        ImageUrl = p.ImageUrl,
        Price = p.Price,
        StockQuantity = p.StockQuantity
    };

    private static void Validate(string barcode, string name, string category, decimal price, int stockQuantity)
    {
        if (string.IsNullOrWhiteSpace(barcode)) throw new ValidationException("Barcode is required");
        if (string.IsNullOrWhiteSpace(name)) throw new ValidationException("Name is required");
        if (string.IsNullOrWhiteSpace(category)) throw new ValidationException("Category is required");
        if (price < 0) throw new ValidationException("Price must be >= 0");
        if (stockQuantity < 0) throw new ValidationException("StockQuantity must be >= 0");
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
