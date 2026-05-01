using BFF.Shared.DTOs;

namespace BFF.Mobile.Endpoints;

// WHY: Mobile order detail shows less info than web — no product images,
// just the essential line items. Saves bandwidth on mobile connections.
public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/mobile/orders");

        group.MapGet("/{orderId}", async (
            int orderId,
            IHttpClientFactory httpClientFactory,
            ILogger<Program> logger) =>
        {
            var orderClient = httpClientFactory.CreateClient("OrderService");

            var order = await orderClient.GetFromJsonAsync<OrderDto>($"/api/orders/{orderId}");
            if (order is null)
                return Results.NotFound();

            // Simplified response for mobile — no product enrichment
            var response = new
            {
                order.Id,
                order.Status,
                Date = order.CreatedAt,
                order.TotalAmount,
                Items = order.Items.Select(i => new
                {
                    i.ProductId,
                    i.Quantity,
                    i.UnitPrice,
                    LineTotal = i.Quantity * i.UnitPrice
                })
            };

            return Results.Ok(response);
        });

        group.MapGet("/user/{userId}", async (
            int userId,
            IHttpClientFactory httpClientFactory) =>
        {
            var orderClient = httpClientFactory.CreateClient("OrderService");
            var orders = await orderClient.GetFromJsonAsync<List<OrderDto>>($"/api/orders/user/{userId}");

            var response = orders?.Select(o => new
            {
                o.Id,
                o.Status,
                Date = o.CreatedAt,
                o.TotalAmount,
                ItemCount = o.Items.Sum(i => i.Quantity)
            });

            return Results.Ok(response);
        });
    }
}
