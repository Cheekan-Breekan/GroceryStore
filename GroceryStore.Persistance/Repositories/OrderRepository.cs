using GroceryStore.Core.DTO;
using GroceryStore.Core.Entities;
using GroceryStore.Core.Interfaces.Repositories;
using GroceryStore.Persistance.Repositories.Cache;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace GroceryStore.Persistance.Repositories;
public class OrderRepository(AppBaseDbContext db, IDistributedCache cache, ILogger<OrderRepository> logger) : IOrderRepository
{
    private readonly AppBaseDbContext _db = db;
    private readonly IDistributedCache _cache = cache;
    private readonly ILogger<OrderRepository> _logger = logger;

    public async Task<string> CreateOrderAsync(int cartId)
    {
        var cart = await _db.Carts.Include(c => c.Items).ThenInclude(i => i.Product).FirstOrDefaultAsync(c => c.Id == cartId && !c.IsOrdered);
        if (cart is null || cart.Items?.Count == 0)
        {
            return string.Empty;
        }
        using var transaction = _db.Database.BeginTransaction();
        try
        {
            var order = new Order
            {
                UserId = cart.UserId,
                TotalPrice = cart.Items.Sum(i => i.Product.Price * i.Quantity),
                Items = cart.Items,
                CartId = cartId,
                CreatedAt = DateTime.UtcNow
            };
            foreach (var item in cart.Items)
            {
                item.Product.Quantity -= item.Quantity;
                if (item.Product.Quantity < 0)
                {
                    throw new ArgumentOutOfRangeException($"Product quantity of cartItem:{item.Id} in storage is less than quantity of order!");
                }
                item.Product.OrdersCount++;
                _cache.Remove(CacheNames.Product(item.ProductId));
            }
            cart.IsOrdered = true;

            await _db.Orders.AddAsync(order);
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
            await _cache.RemoveAsync($"Orders:{cart.UserId}");
            await _cache.RemoveAsync(CacheNames.Cart(cart.UserId));
            return order.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.StackTrace);
            return string.Empty;
        }
    }
    public async Task<List<OrderDto>?> GetOrdersListAsync(string userId)
    {
        var orders = await _cache.GetRecordAsync<List<OrderDto>>($"Orders:{userId}");
        if (orders is not null)
        {
            return orders;
        }
        orders = await _db.Orders.Where(o => o.UserId == userId).Select(o => new OrderDto
        {
            Id = o.Id,
            Items = o.Items.Select(i => new CartItemDto
            {
                Id = i.Id,
                CartId = i.CartId,
                ProductId = i.ProductId,
                Quantity = i.Quantity,
            }).ToList(),
            UserId = o.UserId,
            TotalPrice = o.TotalPrice,
            CreatedAt = o.CreatedAt
        }).ToListAsync();
        if (orders is not null)
        {
            await _cache.SetRecordAsync($"Orders:{userId}", orders, TimeSpan.FromDays(7));
        }
        return orders;
    }
    public async Task<OrderDto?> GetOrderDetailsAsync(string orderId, string userId)
    {
        var order = await _cache.GetRecordAsync<OrderDto>($"Order:orderId:{orderId}:userId:{userId}");
        if (order is not null)
        {
            return order;
        }
        order = await _db.Orders.Select(o => new OrderDto
        {
            Id = o.Id,
            Items = o.Items.Select(i => new CartItemDto
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
                    },
                    CategoryId = i.Product.Category.Id,
                    OrdersCount = i.Product.OrdersCount,
                    ImageMainPath = i.Product.ImageMainPath,
                    ImagePaths = i.Product.ImagePaths
                }
            }).ToList(),
            UserId = o.UserId,
            TotalPrice = o.TotalPrice,
            CreatedAt = o.CreatedAt
        }).FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);
        if (order is not null)
        {
            await _cache.SetRecordAsync($"Order:orderId:{orderId}:userId:{userId}", order, TimeSpan.FromDays(7));
        }
        return order;
    }
}
