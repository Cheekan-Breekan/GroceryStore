using GroceryStore.Api.ViewModels;
using GroceryStore.Core.DTO.Auth;
using GroceryStore.Core.Entities;
using GroceryStore.Core.Interfaces.Services;
using GroceryStore.Core.ResultModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Web;

namespace GroceryStore.Api.Controllers;
[Route("api/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly IAuthService _authService;

    public AccountController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost]
    [Route("Register")]
    public async Task<IActionResult> Register(UserRegisterModel user)
    {
        var regResult = await _authService.RegisterAsync(user);
        if (regResult.IsFailure)
        {
            return BadRequest(CreateProblemDetails(regResult));
        }

        var userEntity = regResult.Value;
        bool sendResult = await SendConfirmationEmail(userEntity);

        return sendResult ?
            Ok("Письмо с подтверждением отправлено на адрес электронной почты")
            : BadRequest("Произошла ошибка при отправке письма с ссылкой для подтверждения аккаунта");
    }

    private async Task<bool> SendConfirmationEmail(MarketUser userEntity)
    {
        var code = await _authService.GenerateEmailConfirmationTokenAsync(userEntity);
        var callbackUrl = Url.Action(nameof(ConfirmEmail), "Account",
                                    new { userId = userEntity.Id, code = code }, protocol: HttpContext.Request.Scheme);
        var messageTitle = "Подтвердите ваш аккаунт";
        var messageBody = $"Подтвердите регистрацию в нашем магазине TestAppMarket, перейдя по ссылке: <a href='{callbackUrl}'>link</a>. " +
                        $"Без данного подтверждения ваш аккаунт не будет активирован.{Environment.NewLine}{Environment.NewLine}" +
                        $"{Environment.NewLine}Если вы не создавали аккаунт в нашем магазине, то игнорируйте это письмо и не переходите по ссылке.";
        var sendResult = await _authService.SendEmailAsync(userEntity.Email, messageTitle, messageBody);
        return sendResult;
    }

    [HttpPost("ConfirmEmail")]
    public async Task<IActionResult> ConfirmEmail(string userId, string code)
    {
        var result = await _authService.ConfirmEmailAsync(userId, code);
        return result.IsSuccess ? Ok() : BadRequest(result.Error.Title);
    }

    [HttpGet("ResendEmail")]
    public async Task<IActionResult> ResendEmail(UserEmailModel email)
    {
        var user = await _authService.FindByEmailAsync(email.Email);
        if (user is null)
        {
            return BadRequest("Пользователь не найден");
        }
        var emailResult = await SendConfirmationEmail(user);
        return emailResult ?
            Ok("Письмо с подтверждением отправлено на адрес электронной почты")
            : BadRequest("Произошла ошибка при отправке письма с ссылкой для подтверждения аккаунта");
    }

    [HttpPost]
    [Route("Login")]
    public async Task<IActionResult> Login(UserLoginModel user)
    {
        var result = await _authService.LoginAsync(user);
        if (result.IsFailure)
        {
            return BadRequest(result.Error.Title);
        }
        var userEntity = result.Value;

        var token = await GenerateJwtTokenAsync(userEntity);
        var refreshToken = GenerateRefreshToken();
        await _authService.SetRefreshTokenAsync(refreshToken, DateTime.Now.AddDays(7), userEntity.Id);

        //delete from response identity cookie
        Response.Cookies.Delete(".AspNetCore.Identity.Application");

        return Ok(new { Token = token, RefreshToken = refreshToken });
    }

    [HttpGet]
    [Route("RefreshToken")]
    public async Task<IActionResult> RefreshToken(RefreshTokenVM refreshToken)
    {
        var principal = GetPrincipalFromExpiredToken(refreshToken.AccessToken);
        if (principal?.Identity?.Name is null)
        {
            return Unauthorized("Невалидный access токен");
        }
        var user = await _authService.FindByNameAsync(principal.Identity.Name);
        if (user is null)
        {
            return Unauthorized("Невалидный access токен");
        }
        var check = await _authService.CheckRefreshTokenAsync(refreshToken.RefreshToken, user.Id);
        if (!check)
        {
            return Unauthorized("Невалидный refresh токен");
        }
        var token = await GenerateJwtTokenAsync(user);
        return Ok(new { Token = token });
    }

    [Authorize]
    [HttpGet("GetUserInfo")]
    public async Task<IActionResult> GetUserInfo()
    {
        var username = User?.Identity?.Name;
        var info = await _authService.GetUserInfoAsync(username);
        if (info.IsFailure)
        {
            return BadRequest(info.Error.Title);
        }
        return Ok(info.Value);
    }

    [Authorize]
    [HttpPost("EditUserInfo")]
    public async Task<IActionResult> EditUserInfo(UserInfoModel model)
    {
        var result = await _authService.EditUserInfoAsync(model);
        if (result.IsFailure)
        {
            return BadRequest(CreateProblemDetails(result));
        }
        return Ok();
    }

    [Authorize]
    [HttpPost("ChangePassword")]
    public async Task<IActionResult> ChangePassword(UserChangePasswordModel model)
    {
        var result = await _authService.ChangePasswordAsync(model);
        if (result.IsFailure)
        {
            return BadRequest(CreateProblemDetails(result));
        }
        return Ok();
    }

    [HttpPost("ForgotPassword")]
    public async Task<IActionResult> ForgotPassword(UserEmailModel model)
    {
        var token = await _authService.GenerateForgotPasswordTokenAsync(model);
        if (token.IsFailure)
        {
            return BadRequest(CreateProblemDetails(token));
        }

        var user = await _authService.FindByEmailAsync(model.Email);
        var encodedToken = HttpUtility.UrlEncode(token.Value);
        var callbackUrl = Url.Action(nameof(ResetPassword), "Account",
                                    new { userId = user.Id, token = token.Value }, protocol: HttpContext.Request.Scheme);
        var messageTitle = "Восстановление пароля";
        var messageBody = $@"Вы начали процедуру сброса пароля для своего аккаунта в нашем магазине TestAppMarket. Пройдите по ссылке: <a href='{callbackUrl}'>link</a>. " +
                        $"После перехода по ссылке ваш пароль сбросится и вы сможете ввести новый.{Environment.NewLine}{Environment.NewLine}" +
                        $"{Environment.NewLine}Если вы не сбрасывали пароль аккаунта в нашем магазине, то игнорируйте это письмо и не переходите по ссылке.";
        var sendResult = await _authService.SendEmailAsync(model.Email, messageTitle, messageBody);
        return sendResult ?
            Ok("Письмо с подтверждением сброса пароля отправлено на адрес электронной почты")
            : BadRequest("Произошла ошибка при отправке письма с ссылкой для подтверждения сброса пароля");
    }

    [HttpPost("ResetPassword")]
    public async Task<IActionResult> ResetPassword(UserResetPasswordModel model)
    {
        model.Token = HttpUtility.UrlDecode(model.Token);
        var result = await _authService.ResetPasswordAsync(model);
        if (result.IsFailure)
        {
            return BadRequest(CreateProblemDetails(result));
        }
        return Ok();
    }

    [Authorize]
    [HttpGet("Logout")]
    public async Task<IActionResult> Logout()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        await _authService.LogoutAsync(userId, true);
        return Ok();
    }

    private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidAudience = AuthOptions.Audience,
            ValidIssuer = AuthOptions.Issuer,
            ValidateLifetime = false,
            IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey()
        };
        return new JwtSecurityTokenHandler().ValidateToken(token, tokenValidationParameters, out _);
    }

    private async Task<string> GenerateJwtTokenAsync(MarketUser user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Surname, user.LastName),
            new Claim(ClaimTypes.NameIdentifier, user.Id)
        };
        var roles = await _authService.GetRolesAsync(user);
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }
        var key = AuthOptions.GetSymmetricSecurityKey();
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(AuthOptions.Issuer, AuthOptions.Audience, claims, expires: DateTime.Now.AddHours(12), signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken() => Guid.NewGuid().ToString();

    private static ProblemDetails CreateProblemDetails(Result result)
    {
        var details = new ProblemDetails
        {
            Status = 400,
            Title = result.Error.Title,
        };
        if (result.Error.Details.Count > 0)
        {
            details.Extensions.Add("errors", result.Error.Details);
        }
        return details;
    }
    private static ProblemDetails CreateProblemDetails<T>(Result<T> result)
    {
        var details = new ProblemDetails
        {
            Status = 400,
            Title = result.Error.Title,
        };
        if (result.Error.Details.Count > 0)
        {
            details.Extensions.Add("errors", result.Error.Details);
        }
        return details;
    }
}
