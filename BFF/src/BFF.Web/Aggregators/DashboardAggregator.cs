using BFF.Shared.DTOs;
using BFF.Web.Mapping;
using BFF.Web.Models.Responses;
using BFF.Web.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace BFF.Web.Aggregators;

// =====================================================================
// CONCEPT: API Aggregation — the heart of the BFF pattern
// =====================================================================
// Without a BFF, a web dashboard would need to:
//   1. Call GET /api/users/{id}           (UserService on port 5001)
//   2. Call GET /api/orders/user/{id}     (OrderService on port 5002)
//   3. Call GET /api/products             (ProductService on port 5003)
//
// Problems:
//   - 3 sequential HTTP requests = slow page load
//   - Frontend must know 3 different service URLs
//   - Frontend must handle 3 different error formats
//   - Frontend must merge data client-side
//
// With the BFF:
//   - Frontend makes 1 call to GET /api/dashboard/{userId}
//   - BFF calls all 3 services in parallel (Task.WhenAll)
//   - BFF handles errors with graceful degradation
//   - BFF transforms data into the shape the frontend needs
// =====================================================================

public class DashboardAggregator : IDashboardAggregator
{
    private readonly IUserService _userService;
    private readonly IOrderService _orderService;
    private readonly IProductService _productService;
    private readonly IMemoryCache _cache;
    private readonly ResponseMapper _mapper;
    private readonly ILogger<DashboardAggregator> _logger;

    public DashboardAggregator(
        IUserService userService,
        IOrderService orderService,
        IProductService productService,
        IMemoryCache cache,
        ResponseMapper mapper,
        ILogger<DashboardAggregator> logger)
    {
        _userService = userService;
        _orderService = orderService;
        _productService = productService;
        _cache = cache;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<DashboardResponse> GetDashboardAsync(int userId)
    {
        var cacheKey = $"dashboard:{userId}";
        if (_cache.TryGetValue(cacheKey, out DashboardResponse? cached))
        {
            _logger.LogInformation("Dashboard cache hit for user {UserId}", userId);
            return cached!;
        }

        // Fire all downstream calls IN PARALLEL — a key BFF advantage
        var userTask = _userService.GetUserAsync(userId);
        var ordersTask = _orderService.GetOrdersByUserAsync(userId);
        var productsTask = _productService.GetAllProductsAsync();

        // Graceful degradation: if one service fails, we still return partial data
        UserDto? user = null;
        List<OrderDto> orders = [];
        List<ProductDto> products = [];

        try { user = await userTask; }
        catch (Exception ex) { _logger.LogWarning(ex, "UserService failed for dashboard aggregation"); }

        try { orders = await ordersTask; }
        catch (Exception ex) { _logger.LogWarning(ex, "OrderService failed for dashboard aggregation"); }

        try { products = await productsTask; }
        catch (Exception ex) { _logger.LogWarning(ex, "ProductService failed for dashboard aggregation"); }

        var response = _mapper.ToDashboardResponse(user, orders, products);

        _cache.Set(cacheKey, response, TimeSpan.FromSeconds(30));
        return response;
    }

    public async Task<UserProfileResponse> GetUserProfileAsync(int userId)
    {
        var cacheKey = $"profile:{userId}";
        if (_cache.TryGetValue(cacheKey, out UserProfileResponse? cached))
            return cached!;

        var userTask = _userService.GetUserAsync(userId);
        var ordersTask = _orderService.GetOrdersByUserAsync(userId);

        var user = await userTask;
        var orders = await ordersTask;

        var response = _mapper.ToUserProfileResponse(user, orders);

        _cache.Set(cacheKey, response, TimeSpan.FromMinutes(5));
        return response;
    }

    public async Task<OrderDetailResponse?> GetOrderDetailAsync(int orderId)
    {
        // No caching — order details must always be fresh
        var order = await _orderService.GetOrderAsync(orderId);
        if (order is null) return null;

        // Enrichment: fetch product details for each order item
        var productIds = order.Items.Select(i => i.ProductId).Distinct().ToArray();
        var products = await _productService.GetProductsByIdsAsync(productIds);

        return _mapper.ToOrderDetailResponse(order, products);
    }
}
