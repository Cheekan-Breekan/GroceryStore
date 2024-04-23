using Microsoft.EntityFrameworkCore;

namespace GroceryStore.Persistance;
public class AppPostgreSQLDbContext : AppBaseDbContext
{
    public AppPostgreSQLDbContext(DbContextOptions<AppPostgreSQLDbContext> opts) : base(opts)
    {
    }
}
