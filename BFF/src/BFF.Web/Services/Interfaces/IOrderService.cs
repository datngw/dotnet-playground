using BFF.Shared.DTOs;

namespace BFF.Web.Services.Interfaces;

public interface IOrderService
{
    Task<OrderDto?> GetOrderAsync(int id);
    Task<List<OrderDto>> GetOrdersByUserAsync(int userId);
}
