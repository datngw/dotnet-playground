using HATEOAS.Common;

namespace HATEOAS.Features.Orders;

public class OrderLinkBuilder : ILinkBuilder<Order>
{
  public Resource<Order> Build(Order order, LinkGenerator linker, HttpContext ctx)
  {
    var links = new List<Link>
    {
        new Link(linker.GetUriByName(ctx, "GetOrderById", new { id = order.Id })!, "self", "GET")
    };

    if (order.Status == OrderStatus.Pending)
    {
      links.Add(new Link(linker.GetUriByName(ctx, "PayOrder", new { id = order.Id })!, "pay", "POST"));
      links.Add(new Link(linker.GetUriByName(ctx, "CancelOrder", new { id = order.Id })!, "cancel", "POST"));
    }

    return new Resource<Order>(order, links);
  }
}

