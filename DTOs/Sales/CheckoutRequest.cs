namespace POS.DTOs.Sales;

public class CheckoutRequest
{
    public List<CheckoutItemRequest> Items { get; set; } = new();

    public decimal AmountPaid { get; set; }
}
