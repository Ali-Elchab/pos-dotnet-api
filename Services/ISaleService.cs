using POS.DTOs.Sales;

namespace POS.Services.Sales;

public interface ISaleService
{
    Task<SaleResponse> CheckoutAsync(CheckoutRequest request);

    Task<List<SaleResponse>> GetRecentAsync(int take = 20);
}
