namespace POS.DTOs.Sales;

public class SaleItemResponse
{
    public int ProductId { get; set; }

    public string Barcode { get; set; } = string.Empty;

    public string ProductName { get; set; } = string.Empty;

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    public decimal LineTotal { get; set; }
}
