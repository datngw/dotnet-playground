namespace HATEOAS.Features.Orders;


public sealed class OrderRepository : IOrderRepository
{
  private readonly List<Order> _orders = new();

  public Task<List<Order>> GetAllAsync() => Task.FromResult(_orders.ToList());
  public Task<Order?> GetByIdAsync(Guid id) => Task.FromResult(_orders.FirstOrDefault(o => o.Id == id));
  public Task<Order> AddAsync(Order order)
  {
    order.Id = Guid.NewGuid();
    _orders.Add(order);
    return Task.FromResult(order);
  }
  public Task UpdateAsync(Order order)
  {
    var index = _orders.FindIndex(o => o.Id == order.Id);
    if (index >= 0) _orders[index] = order;
    return Task.CompletedTask;
  }
}