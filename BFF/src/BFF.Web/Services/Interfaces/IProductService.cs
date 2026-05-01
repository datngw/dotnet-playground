using BFF.Shared.DTOs;

namespace BFF.Web.Services.Interfaces;

public interface IProductService
{
    Task<ProductDto?> GetProductAsync(int id);
    Task<List<ProductDto>> GetAllProductsAsync();
    Task<List<ProductDto>> GetProductsByIdsAsync(int[] ids);
}
