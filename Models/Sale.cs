namespace POS.Models;

public class Sale
{
    public int Id { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public decimal Subtotal { get; set; }

    public decimal Tax { get; set; }

    public decimal Total { get; set; }

    public decimal AmountPaid { get; set; }

    public decimal ChangeDue { get; set; }

    public List<SaleItem> Items { get; set; } = new();
}
