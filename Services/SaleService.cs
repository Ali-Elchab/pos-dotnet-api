using Microsoft.EntityFrameworkCore;
using POS.Data;
using POS.DTOs.Sales;
using POS.Models;
using System.ComponentModel.DataAnnotations;

namespace POS.Services.Sales;

public class SaleService : ISaleService
{
    private const decimal TaxRate = 0.11m;
    private readonly AppDbContext _context;

    public SaleService(AppDbContext context) => _context = context;

    public async Task<SaleResponse> CheckoutAsync(CheckoutRequest request)
    {
        if (request.Items.Count == 0)
            throw new ValidationException("At least one item is required");

        var requestedItems = request.Items
            .GroupBy(x => x.ProductId)
            .Select(x => new CheckoutItemRequest
            {
                ProductId = x.Key,
                Quantity = x.Sum(item => item.Quantity)
            })
            .ToList();

        if (requestedItems.Any(x => x.ProductId <= 0 || x.Quantity <= 0))
            throw new ValidationException("ProductId and Quantity must be greater than zero");

        await using var transaction = await _context.Database.BeginTransactionAsync();

        var productIds = requestedItems.Select(x => x.ProductId).ToList();
        var products = await _context.Products
            .Where(x => productIds.Contains(x.Id) && x.IsActive)
            .ToListAsync();

        if (products.Count != productIds.Count)
            throw new ValidationException("One or more products were not found");

        var items = new List<SaleItem>();

        foreach (var requestedItem in requestedItems)
        {
            var product = products.First(x => x.Id == requestedItem.ProductId);

            if (product.StockQuantity < requestedItem.Quantity)
                throw new ValidationException($"Not enough stock for {product.Name}");

            product.StockQuantity -= requestedItem.Quantity;

            items.Add(new SaleItem
            {
                ProductId = product.Id,
                Barcode = product.Barcode,
                ProductName = product.Name,
                Category = product.Category,
                ImageUrl = product.ImageUrl,
                UnitPrice = product.Price,
                Quantity = requestedItem.Quantity,
                LineTotal = product.Price * requestedItem.Quantity
            });
        }

        var subtotal = items.Sum(x => x.LineTotal);
        var tax = Math.Round(subtotal * TaxRate, 2, MidpointRounding.AwayFromZero);
        var total = subtotal + tax;

        if (request.AmountPaid < total)
            throw new ValidationException("AmountPaid must cover the sale total");

        var sale = new Sale
        {
            CreatedAtUtc = DateTime.UtcNow,
            Subtotal = subtotal,
            Tax = tax,
            Total = total,
            AmountPaid = request.AmountPaid,
            ChangeDue = request.AmountPaid - total,
            Items = items
        };

        _context.Sales.Add(sale);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return Map(sale);
    }

    public async Task<List<SaleResponse>> GetRecentAsync(int take = 20)
    {
        take = Math.Clamp(take, 1, 100);

        var sales = await _context.Sales
            .AsNoTracking()
            .Include(x => x.Items)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(take)
            .ToListAsync();

        return sales.Select(Map).ToList();
    }

    private static SaleResponse Map(Sale sale) => new()
    {
        Id = sale.Id,
        CreatedAtUtc = sale.CreatedAtUtc,
        Subtotal = sale.Subtotal,
        Tax = sale.Tax,
        Total = sale.Total,
        AmountPaid = sale.AmountPaid,
        ChangeDue = sale.ChangeDue,
        Items = sale.Items.Select(item => new SaleItemResponse
        {
            ProductId = item.ProductId,
            Barcode = item.Barcode,
            ProductName = item.ProductName,
            Category = item.Category,
            ImageUrl = item.ImageUrl,
            UnitPrice = item.UnitPrice,
            Quantity = item.Quantity,
            LineTotal = item.LineTotal
        }).ToList()
    };
}
