using Microsoft.EntityFrameworkCore;

namespace GroceryStore.Persistance;
public class AppMSSQLDbContext : AppBaseDbContext
{
    public AppMSSQLDbContext(DbContextOptions<AppMSSQLDbContext> opts) : base(opts)
    {

    }
}
