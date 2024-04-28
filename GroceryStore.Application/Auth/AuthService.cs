using GroceryStore.Core.Interfaces.Services;
using GroceryStore.Core.Entities;
using GroceryStore.Core.Interfaces;
using GroceryStore.Core.ResultModels;
using GroceryStore.Core.DTO.Auth;
using GroceryStore.Core.Interfaces.Repositories;
using Microsoft.AspNetCore.Identity;

namespace GroceryStore.Application.Auth;
public class AuthService : IAuthService
{
    private readonly UserManager<MarketUser> _userManager;
    private readonly SignInManager<MarketUser> _signInManager;
    private readonly IEmailSender _emailSender;
    private readonly IUserRepository _userRepository;

    public AuthService(UserManager<MarketUser> userManager, SignInManager<MarketUser> signInManager, IEmailSender emailSender,
        IUserRepository userRepository)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _emailSender = emailSender; _userRepository = userRepository;
    }

    public async Task<Result<MarketUser>> RegisterAsync(UserRegisterModel user)
    {
        if (await _userManager.FindByNameAsync(user.UserName) is not null)
        {
            return Result<MarketUser>.Failure(new Error("Пользователь с таким именем уже существует"));
        }
        var userEntity = new MarketUser
        {
            UserName = user.UserName,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            MiddleName = user.MiddleName,
            BirthDate = new DateOnly(user.BirthDate.Year, user.BirthDate.Month, user.BirthDate.Day)
        };
        var result = await _userManager.CreateAsync(userEntity, user.Password);
        if (!result.Succeeded)
        {
            return Result<MarketUser>.Failure(new Error("При регистрации произошла ошибка", result.Errors.ToDictionary(e => e.Code, e => e.Description)));
        }
        //await _userManager.AddToRoleAsync(userEntity, "Admin"); //TODO: delete
        return Result<MarketUser>.Success(userEntity);
    }

    public async Task<Result> ConfirmEmailAsync(string userId, string code)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(code))
        {
            return Result.Failure(ErrorAuth.IncorrectData);
        }
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return Result.Failure(ErrorAuth.UserNotFound);
        }
        var result = await _userManager.ConfirmEmailAsync(user, code);
        return result.Succeeded ? Result.Success() : Result.Failure(ErrorAuth.IncorrectData);
    }

    public async Task<bool> SendEmailAsync(string email, string messageTitle, string messageBody)
    {
        return await _emailSender.SendEmailAsync(email, messageTitle, messageBody);
    }

    public async Task<Result<MarketUser>> LoginAsync(UserLoginModel user)
    {
        var userEntity = await _userManager.FindByNameAsync(user.UserNameOrEmail) ?? await _userManager.FindByEmailAsync(user.UserNameOrEmail);
        if (userEntity is null)
        {
            return Result<MarketUser>.Failure(ErrorAuth.UserNotFound);
        }
        if (!await _userManager.IsEmailConfirmedAsync(userEntity))
        {
            return Result<MarketUser>.Failure(ErrorAuth.UnconfirmedEmail);
        }
        //Предпочитаю этот метод, а не CheckPasswordAsync, потому что у SingInAsync есть дополнительная встроенная валидация
        var result = await _signInManager.PasswordSignInAsync(userEntity, user.Password, user.IsPersistent, false);

        if (!result.Succeeded)
        {
            return Result<MarketUser>.Failure(ErrorAuth.IncorrectData);
        }
        return Result<MarketUser>.Success(userEntity);
    }

    public async Task<bool> SetRefreshTokenAsync(string refreshToken, DateTime expires, string userId)
    {
        return await _userRepository.SetRefreshTokenAsync(refreshToken, expires, userId);
    }

    public async Task<bool> CheckRefreshTokenAsync(string refreshToken, string userId)
    {
        return await _userRepository.CheckRefreshTokenAsync(refreshToken, userId);
    }

    public async Task<Result> EditUserInfoAsync(UserInfoModel model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user is null)
        {
            return Result.Failure(ErrorAuth.UserNotFound);
        }
        if (model.UserName != user.UserName && await _userManager.FindByNameAsync(model.UserName) is not null)
        {
            return Result.Failure(ErrorAuth.UserAlreadyExists);
        }
        user.UserName = model.UserName;
        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.MiddleName = model.MiddleName;
        user.BirthDate = new DateOnly(model.BirthDate.Year, model.BirthDate.Month, model.BirthDate.Day);
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return Result.Failure(new Error("Данные невозможно обновить.", result.Errors.ToDictionary(e => e.Code, e => e.Description)));
        }
        return Result.Success();
    }

    public async Task<Result<UserInfoModel>> GetUserInfoAsync(string username)
    {
        var user = await _userManager.FindByNameAsync(username);
        if (user is null)
        {
            return Result<UserInfoModel>.Failure(ErrorAuth.UserNotFound);
        }
        var info = new UserInfoModel
        {
            UserName = user.UserName,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            MiddleName = user.MiddleName,
            BirthDate = new DateTime(user.BirthDate.Year, user.BirthDate.Month, user.BirthDate.Day)
        };
        return Result<UserInfoModel>.Success(info);
    }

    public async Task<Result> ChangePasswordAsync(UserChangePasswordModel model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user is null)
        {
            return Result.Failure(ErrorAuth.UserNotFound);
        }
        var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
        if (!result.Succeeded)
        {
            return Result.Failure(new Error("Пароль не был обновлён.", result.Errors.ToDictionary(e => e.Code, e => e.Description)));
        }
        return Result.Success();
    }

    public async Task<Result<string>> GenerateForgotPasswordTokenAsync(UserEmailModel model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user is null)
        {
            return Result<string>.Failure(ErrorAuth.UserNotFound);
        }
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        return Result<string>.Success(token);
    }

    public async Task<Result> ResetPasswordAsync(UserResetPasswordModel model)
    {
        var user = await _userManager.FindByIdAsync(model.Id);
        if (user is null)
        {
            return Result.Failure(ErrorAuth.UserNotFound);
        }
        var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
        if (!result.Succeeded)
        {
            return Result.Failure(new Error("Произошла ошибка при сбросе пароля.", result.Errors.ToDictionary(e => e.Code, e => e.Description)));
        }
        return Result.Success();
    }

    public async Task LogoutAsync(string? userId = null, bool isJwtAuth = false)
    {
        await _signInManager.SignOutAsync();
        if (isJwtAuth)
        {
            await _userRepository.RemoveRefreshTokenAsync(userId);
        }
    }

    public async Task<string> GenerateEmailConfirmationTokenAsync(MarketUser userEntity) => await _userManager.GenerateEmailConfirmationTokenAsync(userEntity);
    public async Task<MarketUser?> FindByEmailAsync(string email) => await _userManager.FindByEmailAsync(email);

    public async Task<MarketUser?> FindByNameAsync(string name) => await _userManager.FindByNameAsync(name);

    public async Task<IList<string>> GetRolesAsync(MarketUser user) => await _userManager.GetRolesAsync(user);

}
