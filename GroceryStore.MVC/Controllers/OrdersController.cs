using GroceryStore.Core.Interfaces.Services;
using GroceryStore.MVC.Extensions;
using GroceryStore.MVC.Filters;
using GroceryStore.MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GroceryStore.MVC.Controllers;
[Authorize]
public class OrdersController : Controller
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [DisplayNotification]
    public async Task<IActionResult> OrdersHistory()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var orders = await _orderService.GetOrdersListAsync(userId);

        return View(orders);
    }

    public async Task<IActionResult> OrderDetails(string orderId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var order = await _orderService.GetOrderDetailsAsync(orderId, userId);
        if (order is null)
        {
            AddTempDataForNotification("Произошла неустановленная ошибка, заказ не был найден!", false);
            return RedirectToAction(nameof(OrdersHistory));
        }
        return View(order);
    }

    public async Task<IActionResult> CreateOrder(int cartId)
    {
        var email = User.FindFirstValue(ClaimTypes.Email);
        var orderId = await _orderService.CreateOrderAsync(cartId, email);
        if (string.IsNullOrEmpty(orderId))
        {
            AddTempDataForNotification("Произошла неустановленная ошибка, заказ не был создан!", false);
            return RedirectToAction(nameof(Index));
        }
        ViewBag.Notification = new NotificationVM("Заказ успешно создан!");
        return View();
    }

    private void AddTempDataForNotification(string message, bool IsSuccessMessage)
    {
        TempData[ControllerExt.NotiNameForTempData] = message;
        if (!IsSuccessMessage) { TempData[ControllerExt.ErrorNameForTempData] = true; }
    }
}
