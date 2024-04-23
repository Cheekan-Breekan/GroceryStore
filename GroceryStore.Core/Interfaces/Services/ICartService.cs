using GroceryStore.Core.DTO;

namespace GroceryStore.Core.Interfaces.Services;
public interface ICartService
{
    Task<bool> AddToCartAsync(string userId, int productId, int productQuantity);
    Task<CartDto?> GetCartAsync(string userId);
    Task<int> ChangeQuantityCartItemAsync(int cartItemId, int quantity, string userId);
    Task<bool> RemoveCartItemAsync(int cartItemId, string userId);
    Task<CartItemDto?> GetCartItemIdAndQuantityAsync(int productId, string userId);
    HashSet<int> GetProductsIdsInCart(string userId);
}
