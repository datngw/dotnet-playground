namespace BFF.Web.Models.Responses;

// WHY: The user profile page needs different data than the dashboard.
// This demonstrates that the BFF creates endpoints per UI view, not per domain.
public class UserProfileResponse
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime MemberSince { get; set; }
    public bool IsActive { get; set; }
    public OrderStats OrderStats { get; set; } = new();
}

public class OrderStats
{
    public int TotalOrders { get; set; }
    public decimal TotalSpent { get; set; }
    public int PendingOrders { get; set; }
}
