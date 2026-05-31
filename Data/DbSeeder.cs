using POS.Data;
using POS.Models;

namespace Pos.Api.Data;

public static class DbSeeder
{
    private const decimal TaxRate = 0.11m;

    public static void Seed(AppDbContext context)
    {
        SeedProducts(context);
        BackfillSaleItems(context);
        SeedSales(context);
    }

    private static void SeedProducts(AppDbContext context)
    {
        var products = new List<Product>
        {
            new()
            {
                Barcode = "111111",
                Name = "Pepsi",
                Category = "Drinks",
                ImageUrl = "/images/products/pepsi.jpg",
                Price = 1.50m,
                StockQuantity = 100
            },
            new()
            {
                Barcode = "222222",
                Name = "Chips",
                Category = "Snacks",
                ImageUrl = "/images/products/chips.jpg",
                Price = 0.75m,
                StockQuantity = 50
            },
            new()
            {
                Barcode = "333333",
                Name = "Water",
                Category = "Drinks",
                ImageUrl = "/images/products/water.jpg",
                Price = 0.50m,
                StockQuantity = 200
            },
            new()
            {
                Barcode = "444444",
                Name = "Chocolate Bar",
                Category = "Snacks",
                ImageUrl = "/images/products/chocolate.jpg",
                Price = 1.25m,
                StockQuantity = 80
            },
            new()
            {
                Barcode = "555555",
                Name = "Coffee",
                Category = "Hot Drinks",
                ImageUrl = "/images/products/coffee.jpg",
                Price = 2.00m,
                StockQuantity = 60
            },
            new()
            {
                Barcode = "666666",
                Name = "Sandwich",
                Category = "Food",
                ImageUrl = "/images/products/sandwich.jpg",
                Price = 3.50m,
                StockQuantity = 35
            },
            new()
            {
                Barcode = "777777",
                Name = "Croissant",
                Category = "Bakery",
                ImageUrl = "/images/products/croissant.jpg",
                Price = 1.75m,
                StockQuantity = 45
            },
            new()
            {
                Barcode = "888888",
                Name = "Orange Juice",
                Category = "Drinks",
                ImageUrl = "/images/products/juice.jpg",
                Price = 2.25m,
                StockQuantity = 70
            },
            new()
            {
                Barcode = "999999",
                Name = "Energy Drink",
                Category = "Drinks",
                ImageUrl = "/images/products/energy.jpg",
                Price = 2.50m,
                StockQuantity = 55
            },
            new()
            {
                Barcode = "101010",
                Name = "Gum",
                Category = "Snacks",
                ImageUrl = "/images/products/gum.jpg",
                Price = 0.60m,
                StockQuantity = 120
            },
            new()
            {
                Barcode = "121212",
                Name = "Milk",
                Category = "Dairy",
                ImageUrl = "/images/products/milk.jpg",
                Price = 1.80m,
                StockQuantity = 40
            },
            new()
            {
                Barcode = "131313",
                Name = "Yogurt",
                Category = "Dairy",
                ImageUrl = "/images/products/yogurt.jpg",
                Price = 1.10m,
                StockQuantity = 65
            }
        };

        var existingProducts = context.Products
            .ToDictionary(product => product.Barcode);
        var existingBarcodes = existingProducts.Keys.ToHashSet();

        foreach (var product in products)
        {
            if (!existingProducts.TryGetValue(product.Barcode, out var existingProduct))
            {
                continue;
            }

            if (existingProduct.Category == "General" || string.IsNullOrWhiteSpace(existingProduct.Category))
            {
                existingProduct.Category = product.Category;
            }

            existingProduct.ImageUrl = product.ImageUrl;
        }

        var missingProducts = products
            .Where(product => !existingBarcodes.Contains(product.Barcode))
            .ToList();

        if (missingProducts.Count == 0)
        {
            return;
        }

        context.Products.AddRange(missingProducts);
        context.SaveChanges();
    }

    private static void BackfillSaleItems(AppDbContext context)
    {
        var productsById = context.Products
            .ToDictionary(product => product.Id);

        var saleItems = context.SaleItems
            .Where(item => item.Category == "General" || item.ImageUrl == null || item.ImageUrl.StartsWith("https://placehold.co"))
            .ToList();

        foreach (var saleItem in saleItems)
        {
            if (!productsById.TryGetValue(saleItem.ProductId, out var product))
            {
                continue;
            }

            if (saleItem.Category == "General" || string.IsNullOrWhiteSpace(saleItem.Category))
            {
                saleItem.Category = product.Category;
            }

            saleItem.ImageUrl = product.ImageUrl;
        }

        if (saleItems.Count > 0)
        {
            context.SaveChanges();
        }
    }

    private static void SeedSales(AppDbContext context)
    {
        var existingSaleCount = context.Sales.Count();
        const int targetDemoSaleCount = 3;

        if (existingSaleCount >= targetDemoSaleCount)
        {
            return;
        }

        var productsByBarcode = context.Products
            .Where(product => product.IsActive)
            .ToDictionary(product => product.Barcode);

        var sales = new List<Sale>
        {
            BuildSale(
                DateTime.UtcNow.AddDays(-2),
                10m,
                productsByBarcode["111111"],
                2,
                productsByBarcode["222222"],
                1),
            BuildSale(
                DateTime.UtcNow.AddDays(-1),
                20m,
                productsByBarcode["333333"],
                4,
                productsByBarcode["444444"],
                2,
                productsByBarcode["555555"],
                1),
            BuildSale(
                DateTime.UtcNow.AddHours(-3),
                15m,
                productsByBarcode["666666"],
                2,
                productsByBarcode["333333"],
                1)
        }
            .Take(targetDemoSaleCount - existingSaleCount)
            .ToList();

        foreach (var sale in sales)
        {
            foreach (var item in sale.Items)
            {
                productsByBarcode[item.Barcode].StockQuantity -= item.Quantity;
            }
        }

        context.Sales.AddRange(sales);
        context.SaveChanges();
    }

    private static Sale BuildSale(DateTime createdAtUtc, decimal amountPaid, params object[] productQuantityPairs)
    {
        var items = new List<SaleItem>();

        for (var i = 0; i < productQuantityPairs.Length; i += 2)
        {
            var product = (Product)productQuantityPairs[i];
            var quantity = (int)productQuantityPairs[i + 1];

            items.Add(new SaleItem
            {
                ProductId = product.Id,
                Barcode = product.Barcode,
                ProductName = product.Name,
                Category = product.Category,
                ImageUrl = product.ImageUrl,
                UnitPrice = product.Price,
                Quantity = quantity,
                LineTotal = product.Price * quantity
            });
        }

        var subtotal = items.Sum(item => item.LineTotal);
        var tax = Math.Round(subtotal * TaxRate, 2, MidpointRounding.AwayFromZero);
        var total = subtotal + tax;

        return new Sale
        {
            CreatedAtUtc = createdAtUtc,
            Subtotal = subtotal,
            Tax = tax,
            Total = total,
            AmountPaid = amountPaid,
            ChangeDue = amountPaid - total,
            Items = items
        };
    }
}
