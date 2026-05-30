using Microsoft.EntityFrameworkCore;
using POS.Data;
using POS.DTOs.Products;

namespace POS.Services.Products;

public class ProductService : IProductService
{
    private readonly AppDbContext _context;

    public ProductService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProductResponse>> GetAllAsync()
    {
        return await _context.Products
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
}