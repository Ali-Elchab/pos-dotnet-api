using POS.DTOs.Products;

namespace POS.Services.Products;

public interface IProductService
{
    Task<List<ProductResponse>> GetAllAsync(string? barcode = null);

    Task<ProductResponse?> GetByIdAsync(int id);

    Task<ProductResponse> CreateAsync(ProductCreateRequest request);

    Task<ProductResponse?> UpdateAsync(int id, ProductUpdateRequest request);

    Task<bool> SoftDeleteAsync(int id);
}
