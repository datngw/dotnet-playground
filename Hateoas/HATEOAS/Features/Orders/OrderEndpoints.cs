using HATEOAS.Common;

namespace HATEOAS.Features.Orders;

public static class OrderEndpoints
{
  public static void MapOrderEndpoints(this IEndpointRouteBuilder app)
  {
    app.MapGet("/orders", async (IOrderService service, ILinkBuilder<Order> builder, HttpContext ctx, LinkGenerator linker) =>
    {
      var orders = await service.GetAllAsync();
      return Results.Ok(orders.Select(o => builder.Build(o, linker, ctx)));
    }).WithName("GetAllOrders");

    app.MapGet("/orders/{id}", async (Guid id, IOrderService service, ILinkBuilder<Order> builder, HttpContext ctx, LinkGenerator linker) =>
    {
      var order = await service.GetByIdAsync(id);
      return order is null ? Results.NotFound() : Results.Ok(builder.Build(order, linker, ctx));
    }).WithName("GetOrderById");

    app.MapPost("/orders", async (Order order, IOrderService service, ILinkBuilder<Order> builder, HttpContext ctx, LinkGenerator linker) =>
    {
      var created = await service.CreateAsync(order);
      return Results.Created(linker.GetUriByName(ctx, "GetOrderById", new { id = created.Id })!, builder.Build(created, linker, ctx));
    }).WithName("CreateOrder");

    app.MapPost("/orders/{id}/pay", async (Guid id, IOrderService service, ILinkBuilder<Order> builder, HttpContext ctx, LinkGenerator linker) =>
    {
      var order = await service.PayAsync(id);
      return order is null ? Results.BadRequest() : Results.Ok(builder.Build(order, linker, ctx));
    }).WithName("PayOrder");

    app.MapPost("/orders/{id}/cancel", async (Guid id, IOrderService service, ILinkBuilder<Order> builder, HttpContext ctx, LinkGenerator linker) =>
    {
      var order = await service.CancelAsync(id);
      return order is null ? Results.BadRequest() : Results.Ok(builder.Build(order, linker, ctx));
    }).WithName("CancelOrder");
  }
}
