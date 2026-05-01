using System.Net;
using BFF.Shared.DTOs;
using BFF.Web.Services.Interfaces;

namespace BFF.Web.Services;

public class OrderBffService : IOrderService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OrderBffService> _logger;

    public OrderBffService(IHttpClientFactory httpClientFactory, ILogger<OrderBffService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("OrderService");
        _logger = logger;
    }

    public async Task<OrderDto?> GetOrderAsync(int id)
    {
        _logger.LogInformation("BFF calling OrderService: GET /api/orders/{Id}", id);

        var response = await _httpClient.GetAsync($"/api/orders/{id}");

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<OrderDto>();
    }

    public async Task<List<OrderDto>> GetOrdersByUserAsync(int userId)
    {
        _logger.LogInformation("BFF calling OrderService: GET /api/orders/user/{UserId}", userId);

        var response = await _httpClient.GetAsync($"/api/orders/user/{userId}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<OrderDto>>() ?? [];
    }
}
