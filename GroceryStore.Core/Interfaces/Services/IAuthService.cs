using GroceryStore.Core.DTO.Auth;
using GroceryStore.Core.Entities;
using GroceryStore.Core.ResultModels;

namespace GroceryStore.Core.Interfaces.Services;
public interface IAuthService
{
    Task<Result<MarketUser>> RegisterAsync(UserRegisterModel user);
    Task<string> GenerateEmailConfirmationTokenAsync(MarketUser userEntity);
    Task<bool> SendEmailAsync(string email, string messageTitle, string messageBody);
    Task<Result> ConfirmEmailAsync(string userId, string code);
    Task<MarketUser?> FindByEmailAsync(string email);
    Task<MarketUser?> FindByNameAsync(string name);
    Task<IList<string>> GetRolesAsync(MarketUser user);
    Task<Result<MarketUser>> LoginAsync(UserLoginModel user);
    Task<bool> SetRefreshTokenAsync(string refreshToken, DateTime expires, string userId);
    Task<bool> CheckRefreshTokenAsync(string refreshToken, string userId);
    Task<Result<UserInfoModel>> GetUserInfoAsync(string username);
    Task<Result> EditUserInfoAsync(UserInfoModel model);
    Task<Result> ChangePasswordAsync(UserChangePasswordModel model);
    Task<Result<string>> GenerateForgotPasswordTokenAsync(UserEmailModel model);
    Task<Result> ResetPasswordAsync(UserResetPasswordModel model);
    Task LogoutAsync(string? userId = null, bool isJwtAuth = false);
}
