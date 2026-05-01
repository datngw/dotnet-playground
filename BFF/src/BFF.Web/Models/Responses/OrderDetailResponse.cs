namespace BFF.Web.Models.Responses;

// WHY: Order items from the OrderService only have ProductId and UnitPrice.
// The BFF enriches them with product names and images from the ProductService.
// This "enrichment" pattern is a core BFF responsibility.
public class OrderDetailResponse
{
    public int OrderId { get; set; }
    public DateTime Date { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public IEnumerable<EnrichedOrderItem> Items { get; set; } = [];
}

public class EnrichedOrderItem
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductImage { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal => Quantity * UnitPrice;
}
