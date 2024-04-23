using GroceryStore.Core.DTO;
using GroceryStore.Core.Interfaces.Repositories;
using GroceryStore.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace GroceryStore.Application.Services;
public class CartService : ICartService
{
    private readonly ICartRepository _cartRepo;
    private readonly ILogger<CartService> _logger;

    public CartService(ICartRepository cartRepository, ILogger<CartService> logger)
    {
        _cartRepo = cartRepository;
        _logger = logger;
    }
    public async Task<bool> AddToCartAsync(string userId, int productId, int productQuantity)
    {
        try
        {
            return await _cartRepo.AddToCartAsync(userId, productId, productQuantity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.StackTrace);
            return false;
        }
    }
    public async Task<CartDto?> GetCartAsync(string userId)
    {
        try
        {
            return await _cartRepo.GetCartAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.StackTrace);
            return null;
        }
    }
    public async Task<int> ChangeQuantityCartItemAsync(int cartItemId, int quantity, string userId)
    {
        try
        {
            return await _cartRepo.ChangeQuantityCartItemAsync(cartItemId, quantity, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.StackTrace);
            return -1;
        }
    }
    public async Task<bool> RemoveCartItemAsync(int cartItemId, string userId)
    {
        try
        {
            return await _cartRepo.RemoveCartItemAsync(cartItemId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.StackTrace);
            return false;
        }
    }
    public async Task<CartItemDto?> GetCartItemIdAndQuantityAsync(int productId, string userId)
    {
        try
        {
            return await _cartRepo.GetCartItemIdAndQuantityAsync(productId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.StackTrace);
            return null;
        }
    }
    public HashSet<int> GetProductsIdsInCart(string userId)
    {
        try
        {
            return _cartRepo.GetProductsIdsInCart(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.StackTrace);
            return new HashSet<int>();
        }
    }
}
