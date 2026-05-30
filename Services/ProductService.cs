using Microsoft.EntityFrameworkCore;
using POS.Data;
using POS.DTOs.Products;
using POS.Models;
using System.ComponentModel.DataAnnotations;

namespace POS.Services.Products;

public class ProductService : IProductService
{
    private readonly AppDbContext _context;

    public ProductService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProductResponse>> GetAllAsync(string? barcode = null)
    {
        var query = _context.Products.AsQueryable();

        // only active products
        query = query.Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(barcode))
        {
            query = query.Where(p => p.Barcode == barcode);
        }

        return await query
            .Select(product => new ProductResponse
            {
                Id = product.Id,
                Barcode = product.Barcode,
                Name = product.Name,
                Price = product.Price,
                StockQuantity = product.StockQuantity
            })
            .ToListAsync();
    }

    public async Task<ProductResponse?> GetByIdAsync(int id)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
        if (product == null) return null;

        return Map(product);
    }

    // Barcode lookup consolidated into GetAllAsync via optional query parameter

    public async Task<ProductResponse> CreateAsync(ProductCreateRequest request)
    {
        ValidateRequest(request.Barcode, request.Name, request.Price, request.StockQuantity);

        // check unique barcode
        if (await _context.Products.AnyAsync(p => p.Barcode == request.Barcode))
            throw new ValidationException("Barcode must be unique");

        var product = new Product
        {
            Barcode = request.Barcode,
            Name = request.Name,
            Price = request.Price,
            StockQuantity = request.StockQuantity,
            IsActive = true
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return Map(product);
    }

    public async Task<ProductResponse?> UpdateAsync(int id, ProductUpdateRequest request)
    {
        ValidateRequest(request.Barcode, request.Name, request.Price, request.StockQuantity);

        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
        if (product == null) return null;

        // check unique barcode excluding current
        if (await _context.Products.AnyAsync(p => p.Barcode == request.Barcode && p.Id != id))
            throw new ValidationException("Barcode must be unique");

        product.Barcode = request.Barcode;
        product.Name = request.Name;
        product.Price = request.Price;
        product.StockQuantity = request.StockQuantity;

        _context.Products.Update(product);
        await _context.SaveChangesAsync();

        return Map(product);
    }

    public async Task<bool> SoftDeleteAsync(int id)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
        if (product == null) return false;

        product.IsActive = false;
        _context.Products.Update(product);
        await _context.SaveChangesAsync();

        return true;
    }

    private static ProductResponse Map(Product product)
    {
        return new ProductResponse
        {
            Id = product.Id,
            Barcode = product.Barcode,
            Name = product.Name,
            Price = product.Price,
            StockQuantity = product.StockQuantity
        };
    }

    private static void ValidateRequest(string barcode, string name, decimal price, int stockQuantity)
    {
        if (string.IsNullOrWhiteSpace(barcode)) throw new ValidationException("Barcode is required");
        if (string.IsNullOrWhiteSpace(name)) throw new ValidationException("Name is required");
        if (price < 0) throw new ValidationException("Price must be >= 0");
        if (stockQuantity < 0) throw new ValidationException("StockQuantity must be >= 0");
    }
}