using BFF.Shared.DTOs;
using BFF.Web.Models.Responses;

namespace BFF.Web.Mapping;

// WHY: The BFF transforms backend data into the shape each frontend needs.
// The Web dashboard needs flattened user info, order summaries, and product
// recommendations — none of which exist as-is in any single microservice.
// This mapping is a core BFF responsibility.
public class ResponseMapper
{
    public DashboardResponse ToDashboardResponse(
        UserDto? user,
        List<OrderDto> orders,
        List<ProductDto> products)
    {
        return new DashboardResponse
        {
            User = new UserInfo
            {
                Name = $"{user?.FirstName} {user?.LastName}",
                Email = user?.Email ?? string.Empty,
                MemberSince = user?.CreatedAt ?? DateTime.MinValue
            },
            RecentOrders = orders
                .OrderByDescending(o => o.CreatedAt)
                .Take(5)
                .Select(o => new OrderSummary
                {
                    OrderId = o.Id,
                    Date = o.CreatedAt,
                    Total = o.TotalAmount,
                    Status = o.Status,
                    ItemCount = o.Items.Sum(i => i.Quantity)
                }),
            RecommendedProducts = products
                .Take(3)
                .Select(p => new ProductRecommendation
                {
                    Name = p.Name,
                    Price = p.Price,
                    ImageUrl = p.ImageUrl
                }),
            TotalOrders = orders.Count,
            TotalSpent = orders
                .Where(o => o.Status != "Cancelled")
                .Sum(o => o.TotalAmount)
        };
    }

    public UserProfileResponse ToUserProfileResponse(UserDto? user, List<OrderDto> orders)
    {
        return new UserProfileResponse
        {
            Name = $"{user?.FirstName} {user?.LastName}",
            Email = user?.Email ?? string.Empty,
            Role = user?.Role ?? string.Empty,
            MemberSince = user?.CreatedAt ?? DateTime.MinValue,
            IsActive = user?.IsActive ?? false,
            OrderStats = new OrderStats
            {
                TotalOrders = orders.Count,
                TotalSpent = orders.Where(o => o.Status != "Cancelled").Sum(o => o.TotalAmount),
                PendingOrders = orders.Count(o => o.Status is "Pending" or "Processing")
            }
        };
    }

    public OrderDetailResponse ToOrderDetailResponse(OrderDto order, List<ProductDto> products)
    {
        var productLookup = products.ToDictionary(p => p.Id);

        return new OrderDetailResponse
        {
            OrderId = order.Id,
            Date = order.CreatedAt,
            Status = order.Status,
            TotalAmount = order.TotalAmount,
            Items = order.Items.Select(item =>
            {
                var product = productLookup.GetValueOrDefault(item.ProductId);
                return new EnrichedOrderItem
                {
                    ProductId = item.ProductId,
                    ProductName = product?.Name ?? "Unknown Product",
                    ProductImage = product?.ImageUrl ?? string.Empty,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                };
            })
        };
    }
}
