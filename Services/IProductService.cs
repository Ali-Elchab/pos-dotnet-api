using POS.DTOs.Products;

namespace POS.Services.Products;

public interface IProductService
{
    Task<List<ProductResponse>> GetAllAsync();
}