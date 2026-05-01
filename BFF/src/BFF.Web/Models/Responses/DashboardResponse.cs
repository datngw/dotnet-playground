namespace BFF.Web.Models.Responses;

// WHY: This response shape is tailored for the Web frontend's dashboard.
// It's NOT a 1:1 copy of any backend service model — the BFF composes
// and transforms data from multiple services into exactly what the UI needs.
public class DashboardResponse
{
    public UserInfo User { get; set; } = new();
    public IEnumerable<OrderSummary> RecentOrders { get; set; } = [];
    public IEnumerable<ProductRecommendation> RecommendedProducts { get; set; } = [];
    public int TotalOrders { get; set; }
    public decimal TotalSpent { get; set; }
}

public class UserInfo
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime MemberSince { get; set; }
}

public class OrderSummary
{
    public int OrderId { get; set; }
    public DateTime Date { get; set; }
    public decimal Total { get; set; }
    public string Status { get; set; } = string.Empty;
    public int ItemCount { get; set; }
}

public class ProductRecommendation
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
}
