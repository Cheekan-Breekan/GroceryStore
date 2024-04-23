using GroceryStore.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GroceryStore.Api.Controllers;
[Route("api/[controller]")]
[ApiController]
public class CartsController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartsController(ICartService cartService)
    {
        _cartService = cartService;
    }

    [HttpGet("GetCart")]
    public async Task<IActionResult> GetCart()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var cart = await _cartService.GetCartAsync(userId);
        return Ok(cart);
    }

    [HttpGet("GetCartItem")]
    public async Task<IActionResult> GetCartItem(int productId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var cartItem = await _cartService.GetCartItemIdAndQuantityAsync(productId, userId);
        if (cartItem is null)
        {
            return NotFound("Данный товар в корзине не найден");
        }
        return Ok(cartItem);
    }

    [HttpGet("AddProductToCart")]
    public async Task<IActionResult> AddProductToCart(int productId, int productQuantity)
    {
        if (productQuantity < 1 || productQuantity > 100)
        {
            return BadRequest("Количество продукта должно быть от 1 до 100");
        }
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _cartService.AddToCartAsync(userId, productId, productQuantity);
        if (!result)
        {
            return BadRequest("Произошла ошибка при добавлении в корзину");
        }
        return Ok();
    }

    [HttpGet("UpdateCartItemQuantity")]
    public async Task<IActionResult> UpdateCartItemQuantity(int cartItemId, int quantityChange, int currentValue)
    {
        if (quantityChange < 1 || quantityChange > 100)
        {
            return BadRequest("Количество продукта должно быть от 1 до 100");
        }
        if (currentValue + quantityChange < 1 || currentValue + quantityChange > 100)
        {
            return BadRequest("Количество продукта в корзине должно быть от 1 до 100");
        }
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var newQuantity = await _cartService.ChangeQuantityCartItemAsync(cartItemId, quantityChange, userId);
        if (newQuantity == currentValue)
        {
            return BadRequest("Не удалось изменить количество продукта в корзине.");
        }
        return Ok(newQuantity);
    }

    [HttpGet("RemoveCartItem")]
    public async Task<IActionResult> RemoveCartItem(int cartItemId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await _cartService.RemoveCartItemAsync(cartItemId, userId);
        if (!result)
        {
            return BadRequest("Не удалось удалить товар из корзины.");
        }
        return Ok();
    }
}
