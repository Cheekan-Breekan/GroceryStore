using GroceryStore.Core.Interfaces.Repositories;

namespace GroceryStore.Persistance.Repositories;
public class UserRepository(AppBaseDbContext db) : IUserRepository
{
    private readonly AppBaseDbContext _db = db;

    public async Task<bool> SetRefreshTokenAsync(string token, DateTime expires, string userId)
    {
        var user = await _db.Users.FindAsync(userId) ?? throw new ArgumentNullException("Can't set a refresh token to user.");
        user.RefreshToken = token;
        user.RefreshTokenExpiryTime = expires.ToUniversalTime(); //postgres demand UTC
        return await _db.SaveChangesAsync() > 0;
    }

    public async Task<bool> CheckRefreshTokenAsync(string token, string userId)
    {
        var user = await _db.Users.FindAsync(userId) ?? throw new ArgumentNullException("Can't find user.");
        return user.RefreshToken == token && user.RefreshTokenExpiryTime > DateTime.UtcNow; //postgres demand UTC
    }

    public async Task RemoveRefreshTokenAsync(string userId)
    {
        var user = await _db.Users.FindAsync(userId);
        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;
        await _db.SaveChangesAsync();
    }
}
