using GroceryStore.Core.Interfaces.Services;
using GroceryStore.MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GroceryStore.MVC.Controllers;
[Authorize]
public class CartsController : Controller
{
    private readonly ICartService _cartService;

    public CartsController(ICartService cartService)
    {
        _cartService = cartService;
    }

    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var cart = await _cartService.GetCartAsync(userId);
        return View(cart);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateCartItemQuantity(int cartItemId, int quantityChange, int currentValue)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _cartService.ChangeQuantityCartItemAsync(cartItemId, quantityChange, userId);
        result = result < 0 ? currentValue : result;
        var cartItem = new CartItemQuantityVM
        {
            Quantity = result,
            CartItemId = cartItemId
        };
        return PartialView("_CartItemQuantityPartial", cartItem);
    }

    [HttpPost]
    public async Task<IActionResult> RemoveCartItem(int cartItemId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _cartService.RemoveCartItemAsync(cartItemId, userId);
        if (!result)
        {
            return BadRequest("Не удалось удалить товар из корзины");
        }
        return Ok();
    }
}
