using POS.Data;
using POS.Models;

namespace Pos.Api.Data;

public static class DbSeeder
{
    public static void Seed(AppDbContext context)
    {
        if (context.Products.Any())
        {
            return;
        }

        var products = new List<Product>
        {
            new Product
            {
                Barcode = "111111",
                Name = "Pepsi",
                Price = 1.50m,
                StockQuantity = 100
            },
            new Product
            {
                Barcode = "222222",
                Name = "Chips",
                Price = 0.75m,
                StockQuantity = 50
            },
            new Product
            {
                Barcode = "333333",
                Name = "Water",
                Price = 0.50m,
                StockQuantity = 200
            }
        };

        context.Products.AddRange(products);
        context.SaveChanges();
    }
}