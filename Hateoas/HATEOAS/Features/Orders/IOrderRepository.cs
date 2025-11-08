namespace HATEOAS.Features.Orders;

public interface IOrderRepository
{
  Task<Order?> GetByIdAsync(Guid id);
  Task<List<Order>> GetAllAsync();
  Task<Order> AddAsync(Order order);
  Task UpdateAsync(Order order);
}