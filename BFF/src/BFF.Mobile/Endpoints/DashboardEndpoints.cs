using BFF.Shared.DTOs;
using Microsoft.Extensions.Caching.Memory;

namespace BFF.Mobile.Endpoints;

// =====================================================================
// CONCEPT: Different BFF per Frontend
// =====================================================================
// WHY is this different from BFF.Web?
// Mobile devices have:
//   - Smaller screens → less data per view
//   - Intermittent connectivity → more aggressive caching
//   - Battery concerns → fewer, smaller requests
//
// The Mobile BFF returns SMALLER payloads than the Web BFF
// for the same conceptual "dashboard" view.
//
// Compare this with BFF.Web's DashboardController to see the difference:
//   - Web: 5 recent orders + 3 product recommendations + stats
//   - Mobile: 3 recent orders only, no recommendations
// =====================================================================

public static class DashboardEndpoints
{
    public static void MapDashboardEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/mobile/dashboard");

        group.MapGet("/{userId}", async (
            int userId,
            IHttpClientFactory httpClientFactory,
            IMemoryCache cache,
            ILogger<Program> logger) =>
        {
            var cacheKey = $"mobile:dashboard:{userId}";
            if (cache.TryGetValue(cacheKey, out object? cached))
                return Results.Ok(cached);

            var userClient = httpClientFactory.CreateClient("UserService");
            var orderClient = httpClientFactory.CreateClient("OrderService");

            // Mobile only needs user + last 3 orders — no product recommendations
            var userTask = userClient.GetFromJsonAsync<UserDto>($"/api/users/{userId}");
            var ordersTask = orderClient.GetFromJsonAsync<List<OrderDto>>($"/api/orders/user/{userId}");

            UserDto? user = null;
            List<OrderDto>? orders = null;

            try { user = await userTask; }
            catch (Exception ex) { logger.LogWarning(ex, "UserService unavailable for mobile dashboard"); }

            try { orders = await ordersTask; }
            catch (Exception ex) { logger.LogWarning(ex, "OrderService unavailable for mobile dashboard"); }

            var response = new
            {
                Name = $"{user?.FirstName} {user?.LastName}",
                Email = user?.Email,
                RecentOrders = orders?
                    .OrderByDescending(o => o.CreatedAt)
                    .Take(3)
                    .Select(o => new
                    {
                        o.Id,
                        Total = o.TotalAmount,
                        Status = o.Status,
                        Date = o.CreatedAt
                    }),
                // Computed stats — same concept as Web BFF but fewer fields
                TotalOrders = orders?.Count ?? 0
            };

            // More aggressive caching for mobile (5 min vs 30s for web)
            cache.Set(cacheKey, response, TimeSpan.FromMinutes(5));

            return Results.Ok(response);
        });
    }
}
