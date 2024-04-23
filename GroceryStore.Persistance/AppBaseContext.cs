using GroceryStore.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace GroceryStore.Persistance;
public class AppBaseDbContext : IdentityDbContext<MarketUser>
{
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Order> Orders { get; set; }
    public AppBaseDbContext(DbContextOptions opts) : base(opts)
    {

    }
}
