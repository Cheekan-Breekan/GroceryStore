using GroceryStore.Core.DTO;

namespace GroceryStore.Core.Interfaces.Repositories;
public interface IOrderRepository
{
    Task<string> CreateOrderAsync(int cartId);
    Task<List<OrderDto>?> GetOrdersListAsync(string userId);
    Task<OrderDto?> GetOrderDetailsAsync(string orderId, string userId);
}
