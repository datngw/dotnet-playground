using HATEOAS.Features.Products;

namespace HATEOAS.Features.Orders;

public sealed class Order
{
  public Guid Id { get; set; } = Guid.NewGuid();
  public string CustomerName { get; set; } = string.Empty;
  public OrderStatus Status { get; set; } = OrderStatus.Pending;
  public List<Product> Products { get; set; } = new();
  public decimal Total => Products.Sum(p => p.Price);
}