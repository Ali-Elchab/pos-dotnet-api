namespace POS.DTOs.Sales;

public class SaleResponse
{
    public int Id { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public decimal Subtotal { get; set; }

    public decimal Tax { get; set; }

    public decimal Total { get; set; }

    public decimal AmountPaid { get; set; }

    public decimal ChangeDue { get; set; }

    public List<SaleItemResponse> Items { get; set; } = new();
}
