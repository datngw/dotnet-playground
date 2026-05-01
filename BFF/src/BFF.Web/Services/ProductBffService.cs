using System.Net;
using BFF.Shared.DTOs;
using BFF.Web.Services.Interfaces;

namespace BFF.Web.Services;

public class ProductBffService : IProductService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProductBffService> _logger;

    public ProductBffService(IHttpClientFactory httpClientFactory, ILogger<ProductBffService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("ProductService");
        _logger = logger;
    }

    public async Task<ProductDto?> GetProductAsync(int id)
    {
        _logger.LogInformation("BFF calling ProductService: GET /api/products/{Id}", id);

        var response = await _httpClient.GetAsync($"/api/products/{id}");

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<ProductDto>();
    }

    public async Task<List<ProductDto>> GetAllProductsAsync()
    {
        _logger.LogInformation("BFF calling ProductService: GET /api/products");

        var response = await _httpClient.GetAsync("/api/products");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<ProductDto>>() ?? [];
    }

    // Batch fetch — used by OrderSummaryController to enrich order items with product details
    public async Task<List<ProductDto>> GetProductsByIdsAsync(int[] ids)
    {
        var idsParam = string.Join(",", ids);
        _logger.LogInformation("BFF calling ProductService: GET /api/products/batch?ids={Ids}", idsParam);

        var response = await _httpClient.GetAsync($"/api/products/batch?ids={idsParam}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<ProductDto>>() ?? [];
    }
}
