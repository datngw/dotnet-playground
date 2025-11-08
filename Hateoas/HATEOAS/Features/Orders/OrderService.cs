namespace HATEOAS.Features.Orders;

public interface IOrderService
{
  Task<List<Order>> GetAllAsync();
  Task<Order?> GetByIdAsync(Guid id);
  Task<Order> CreateAsync(Order order);
  Task<Order?> PayAsync(Guid id);
  Task<Order?> CancelAsync(Guid id);
}

public class OrderService : IOrderService
{
  private readonly IOrderRepository _repo;
  public OrderService(IOrderRepository repo) => _repo = repo;

  public Task<List<Order>> GetAllAsync() => _repo.GetAllAsync();
  public Task<Order?> GetByIdAsync(Guid id) => _repo.GetByIdAsync(id);

  public Task<Order> CreateAsync(Order order) => _repo.AddAsync(order);

  public async Task<Order?> PayAsync(Guid id)
  {
    var order = await _repo.GetByIdAsync(id);
    if (order == null || order.Status != OrderStatus.Pending) return null;
    order.Status = OrderStatus.Paid;
    await _repo.UpdateAsync(order);
    return order;
  }

  public async Task<Order?> CancelAsync(Guid id)
  {
    var order = await _repo.GetByIdAsync(id);
    if (order == null || order.Status != OrderStatus.Pending) return null;
    order.Status = OrderStatus.Cancelled;
    await _repo.UpdateAsync(order);
    return order;
  }
}

