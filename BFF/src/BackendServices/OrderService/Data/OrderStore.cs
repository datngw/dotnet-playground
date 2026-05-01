using OrderService.Models;

namespace OrderService.Data;

public class OrderStore
{
    private readonly List<Order> _orders =
    [
        new() { Id = 1, UserId = 1, Status = "Delivered", CreatedAt = new DateTime(2024, 10, 1), TotalAmount = 259.98m, Items = [new() { ProductId = 1, Quantity = 1, UnitPrice = 199.99m }, new() { ProductId = 4, Quantity = 1, UnitPrice = 59.99m }] },
        new() { Id = 2, UserId = 1, Status = "Shipped", CreatedAt = new DateTime(2024, 11, 15), TotalAmount = 34.99m, Items = [new() { ProductId = 5, Quantity = 1, UnitPrice = 34.99m }] },
        new() { Id = 3, UserId = 1, Status = "Processing", CreatedAt = new DateTime(2024, 12, 20), TotalAmount = 89.97m, Items = [new() { ProductId = 3, Quantity = 3, UnitPrice = 29.99m }] },
        new() { Id = 4, UserId = 2, Status = "Delivered", CreatedAt = new DateTime(2024, 9, 10), TotalAmount = 449.99m, Items = [new() { ProductId = 2, Quantity = 1, UnitPrice = 449.99m }] },
        new() { Id = 5, UserId = 2, Status = "Cancelled", CreatedAt = new DateTime(2024, 10, 5), TotalAmount = 59.99m, Items = [new() { ProductId = 4, Quantity = 1, UnitPrice = 59.99m }] },
        new() { Id = 6, UserId = 3, Status = "Pending", CreatedAt = new DateTime(2024, 12, 28), TotalAmount = 529.97m, Items = [new() { ProductId = 1, Quantity = 1, UnitPrice = 199.99m }, new() { ProductId = 2, Quantity = 1, UnitPrice = 449.99m }] },
        new() { Id = 7, UserId = 5, Status = "Shipped", CreatedAt = new DateTime(2024, 11, 20), TotalAmount = 119.98m, Items = [new() { ProductId = 3, Quantity = 2, UnitPrice = 29.99m }, new() { ProductId = 3, Quantity = 2, UnitPrice = 29.99m }] },
        new() { Id = 8, UserId = 5, Status = "Delivered", CreatedAt = new DateTime(2024, 8, 3), TotalAmount = 79.98m, Items = [new() { ProductId = 7, Quantity = 2, UnitPrice = 39.99m }] },
    ];

    public Task<List<Order>> GetAllAsync() => Task.FromResult(_orders);

    public Task<List<Order>> GetByUserIdAsync(int userId) =>
        Task.FromResult(_orders.Where(o => o.UserId == userId).ToList());

    public Task<Order?> GetByIdAsync(int id) =>
        Task.FromResult(_orders.FirstOrDefault(o => o.Id == id));
}
