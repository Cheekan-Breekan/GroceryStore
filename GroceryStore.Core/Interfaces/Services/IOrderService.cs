using GroceryStore.Core.DTO;

namespace GroceryStore.Core.Interfaces.Services;
public interface IOrderService
{
    Task<string> CreateOrderAsync(int cartId, string email);
    Task<List<OrderDto>?> GetOrdersListAsync(string userId);
    Task<OrderDto?> GetOrderDetailsAsync(string orderId, string userId);
}
