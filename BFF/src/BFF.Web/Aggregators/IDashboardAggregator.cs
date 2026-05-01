using BFF.Web.Models.Responses;

namespace BFF.Web.Aggregators;

public interface IDashboardAggregator
{
    Task<DashboardResponse> GetDashboardAsync(int userId);
    Task<UserProfileResponse> GetUserProfileAsync(int userId);
    Task<OrderDetailResponse?> GetOrderDetailAsync(int orderId);
}
