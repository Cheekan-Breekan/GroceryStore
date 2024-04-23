using GroceryStore.Core.DTO;
using GroceryStore.Core.Interfaces;
using GroceryStore.Core.Interfaces.Repositories;
using GroceryStore.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace GroceryStore.Application.Services;
public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepo;
    private readonly ILogger<OrderService> _logger;
    private readonly IEmailSender _emailSender;

    public OrderService(IOrderRepository orderRepository, ILogger<OrderService> logger, IEmailSender emailSender)
    {
        _orderRepo = orderRepository;
        _orderRepo = orderRepository;
        _logger = logger;
        _emailSender = emailSender;
    }
    public async Task<string> CreateOrderAsync(int cartId, string email)
    {
        try
        {
            var orderId = await _orderRepo.CreateOrderAsync(cartId);
            if (string.IsNullOrEmpty(orderId))
            {
                return string.Empty;
            }
            await _emailSender.SendEmailAsync(email, $"Заказ в TestAppMarket", $"Заказ с идентификатором {orderId} был создан.");
            return orderId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.StackTrace);
            return string.Empty;
        }
    }
    public async Task<List<OrderDto>?> GetOrdersListAsync(string userId)
    {
        try
        {
            return await _orderRepo.GetOrdersListAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.StackTrace);
            return [];
        }
    }
    public async Task<OrderDto?> GetOrderDetailsAsync(string orderId, string userId)
    {
        try
        {
            return await _orderRepo.GetOrderDetailsAsync(orderId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.StackTrace);
            return null;
        }
    }
}
