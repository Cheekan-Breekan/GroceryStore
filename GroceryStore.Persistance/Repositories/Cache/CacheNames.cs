namespace GroceryStore.Persistance.Repositories.Cache;
public static class CacheNames
{
    public static string Product(int id) => $"Product:{id}";
    public static string Category(int id) => $"Category:{id}";
    public static string CartItem(string userId, int productId) => $"CartItem:user:{userId}:product:{productId}";
    public static string Cart(string userId) => $"Cart:user:{userId}";
}
