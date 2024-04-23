using GroceryStore.Core.DTO;
using GroceryStore.Core.Entities;
using GroceryStore.Core.Interfaces.Repositories;
using GroceryStore.Persistance.Repositories.Cache;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace GroceryStore.Persistance.Repositories;
public class CartRepository(AppBaseDbContext db, IDistributedCache cache) : ICartRepository
{
    private readonly AppBaseDbContext _db = db;
    private readonly IDistributedCache _cache = cache;

    public async Task<bool> AddToCartAsync(string userId, int productId, int productQuantity)
    {
        if (productQuantity <= 0 || productQuantity > 100 || string.IsNullOrWhiteSpace(userId) || productId <= 0)
        {
            return false;
        }
        var cart = await _db.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.UserId == userId && !c.IsOrdered);
        if (cart is null)
        {
            cart = new Cart
            {
                UserId = userId,
            };
            await _db.Carts.AddAsync(cart);
        }
        var cartItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        if (cartItem is null)
        {
            cart.Items.Add(new CartItem
            {
                CartId = cart.Id,
                ProductId = productId,
                Quantity = productQuantity,
            });
        }
        else
        {
            var result = await ChangeQuantityCartItemAsync(cartItem.Id, productQuantity, userId);
            return result != cartItem.Quantity;
        }
        await _cache.RemoveAsync(CacheNames.Cart(userId));
        return await _db.SaveChangesAsync() > 0;
    }
    public async Task<CartDto> GetCartAsync(string userId)
    {
        var cart = await _cache.GetRecordAsync<CartDto>(CacheNames.Cart(userId));
        if (cart is not null)
        {
            return cart;
        }
        cart = await _db.Carts.Where(c => c.UserId == userId && !c.IsOrdered).Select(c => new CartDto
        {
            Id = c.Id,
            UserId = userId,
            Items = c.Items.Select(i => new CartItemDto
            {
                Id = i.Id,
                CartId = i.CartId,
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                Product = new ProductDto
                {
                    Id = i.Product.Id,
                    Name = i.Product.Name,
                    Description = i.Product.Description,
                    Price = i.Product.Price,
                    Quantity = i.Product.Quantity,
                    Category = new CategoryDto
                    {
                        Id = i.Product.Category.Id,
                        Name = i.Product.Category.Name,
                        Description = i.Product.Category.Description,
                        ProductsCount = i.Product.Category.Products.Count
                    },
                    CategoryId = i.Product.Category.Id,
                    OrdersCount = i.Product.OrdersCount,
                    ImageMainPath = i.Product.ImageMainPath,
                    ImagePaths = i.Product.ImagePaths
                }
            }).ToList()
        }).FirstOrDefaultAsync();

        if (cart is null)
        {
            var cartEntity = new Cart
            {
                UserId = userId,
            };
            await _db.Carts.AddAsync(cartEntity);
            await _db.SaveChangesAsync();
            return new CartDto
            {
                Id = cartEntity.Id,
                UserId = userId
            };
        }
        await _cache.SetRecordAsync(CacheNames.Cart(userId), cart, TimeSpan.FromMinutes(1));
        return cart;
    }
    public async Task<int> ChangeQuantityCartItemAsync(int cartItemId, int quantity, string userId)
    {
        var cartItem = await _db.CartItems.FindAsync(cartItemId) ?? throw new ArgumentNullException($"CartItem with id:{cartItemId} not found");
        var mathResult = cartItem.Quantity + quantity;
        if (mathResult < 0 || mathResult > 100)
        {
            return cartItem.Quantity;
        }
        cartItem.Quantity = mathResult;
        await _db.SaveChangesAsync();
        await _cache.RemoveAsync(CacheNames.Cart(userId));
        await _cache.RemoveAsync(CacheNames.CartItem(userId, cartItem.ProductId));
        return cartItem.Quantity;
    }
    public async Task<bool> RemoveCartItemAsync(int cartItemId, string userId)
    {
        var cartItem = await _db.CartItems.FindAsync(cartItemId) ?? throw new ArgumentNullException($"CartItem with id:{cartItemId} not found");
        _db.CartItems.Remove(cartItem);
        await _cache.RemoveAsync(CacheNames.Cart(userId));
        await _cache.RemoveAsync(CacheNames.CartItem(userId, cartItem.ProductId));
        return await _db.SaveChangesAsync() > 0;
    }
    public async Task<CartItemDto?> GetCartItemIdAndQuantityAsync(int productId, string userId)
    {
        var cartItem = await _cache.GetRecordAsync<CartItemDto>(CacheNames.CartItem(userId, productId));
        if (cartItem is not null)
        {
            return cartItem;
        }
        cartItem = await _db.Carts
            .Where(c => c.UserId == userId && !c.IsOrdered && c.Items.Any(i => i.ProductId == productId))
            .SelectMany(c => c.Items)
            .Where(i => i.ProductId == productId)
            .Select(i => new CartItemDto
            {
                Id = i.Id,
                Quantity = i.Quantity,
            })
            .FirstOrDefaultAsync();
        if (cartItem is not null)
        {
            await _cache.SetRecordAsync(CacheNames.CartItem(userId, productId), cartItem, TimeSpan.FromMinutes(1));
        }
        return cartItem;
    }
    public HashSet<int> GetProductsIdsInCart(string userId)
    {
        var cart = _db.Carts.Where(c => c.UserId == userId && !c.IsOrdered)
            .SelectMany(c => c.Items)
            .Select(i => i.ProductId)
            .ToHashSet();
        return cart;
    }
}
