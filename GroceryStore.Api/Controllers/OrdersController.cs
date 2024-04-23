using GroceryStore.Core.Interfaces;
using GroceryStore.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GroceryStore.Api.Controllers;
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet("GetOrders")]
    public async Task<IActionResult> GetOrders()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var orders = await _orderService.GetOrdersListAsync(userId);
        return Ok(orders);
    }

    [HttpGet("GetOrderDetails")]
    public async Task<IActionResult> GetOrderDetails(string orderId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var order = await _orderService.GetOrderDetailsAsync(orderId, userId);
        if (order is null)
        {
            return NotFound("Заказ не найден");
        }
        return Ok(order);
    }

    [HttpGet("AddOrder")]
    public async Task<IActionResult> CreateOrder(int cartId)
    {
        var email = User.FindFirstValue(ClaimTypes.Email);
        var orderId = await _orderService.CreateOrderAsync(cartId, email);
        if (string.IsNullOrEmpty(orderId))
        {
            return BadRequest("Произошла ошибка при создании заказа");
        }
        return Ok(orderId);
    }
}
