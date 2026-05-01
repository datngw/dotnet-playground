namespace BFF.Shared.DTOs;

public class OrderDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public List<OrderItemDto> Items { get; set; } = [];
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public decimal TotalAmount { get; set; }
}

public class OrderItemDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
