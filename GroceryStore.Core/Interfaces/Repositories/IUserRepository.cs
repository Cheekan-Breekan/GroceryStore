namespace GroceryStore.Core.Interfaces.Repositories;
public interface IUserRepository
{
    Task<bool> SetRefreshTokenAsync(string token, DateTime expires, string userId);
    Task<bool> CheckRefreshTokenAsync(string token, string userId);
    Task RemoveRefreshTokenAsync(string userId);
}
