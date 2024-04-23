using GroceryStore.Core.DTO.Auth;
using GroceryStore.Core.Entities;
using GroceryStore.Core.Interfaces.Services;
using GroceryStore.Core.ResultModels;
using GroceryStore.MVC.Extensions;
using GroceryStore.MVC.Filters;
using GroceryStore.MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json;

namespace GroceryStore.MVC.Controllers;
public class AccountController : Controller
{
    private readonly IAuthService _authService;
    private const string _modelStateForTempData = "ModelState";

    public AccountController(IAuthService authService)
    {
        _authService = authService;
    }

    [Route("/Account/Profile")]
    [Route("/Account")]
    [HttpGet]
    [Authorize]
    [DisplayNotification]
    public async Task<IActionResult> Profile()
    {
        var username = User?.Identity?.Name;
        var userInfo = await _authService.GetUserInfoAsync(username);
        if (TempData[_modelStateForTempData] is not null)
        {
            var value = TempData[_modelStateForTempData] as string;
            var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(value);
            if (dict is not null)
            {
                foreach (var item in dict)
                {
                    ModelState.TryAddModelError(item.Key, item.Value);
                }
            }
        }
        return View(new UserProfileVM
        {
            UserInfoModel = userInfo.Value,
            UserChangePasswordModel = new UserChangePasswordModel { Email = userInfo.Value.Email }
        });
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Profile(UserInfoModel userInfo)
    {
        if (!ModelState.IsValid)
        {
            AddTempDataForModelStateErrors();
            return RedirectToAction(nameof(Profile));
        }
        var result = await _authService.EditUserInfoAsync(userInfo);
        if (result.IsFailure)
        {
            ModelState.TryAddModelError("Ошибка", result.Error.Title);
            foreach (var error in result.Error.Details)
            {
                ModelState.TryAddModelError(error.Key, error.Value);
            }
            AddTempDataForModelStateErrors();
            return RedirectToAction(nameof(Profile));
        }
        AddTempDataForNotification("Профиль успешно обновлен", true);
        return RedirectToAction(nameof(Profile));
    }
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> ChangePassword(UserChangePasswordModel model)
    {
        if (!ModelState.IsValid)
        {
            AddTempDataForModelStateErrors();
            return RedirectToAction(nameof(Profile));
        }
        var result = await _authService.ChangePasswordAsync(model);
        if (result.IsFailure)
        {
            foreach (var error in result.Error.Details)
            {
                ModelState.TryAddModelError(error.Key, error.Value);
            }
            AddTempDataForModelStateErrors();
            return RedirectToAction(nameof(Profile));
        }
        AddTempDataForNotification("Пароль успешно обновлен", true);
        return RedirectToAction(nameof(Profile));
    }
    [NonAuthorized]
    public IActionResult Register()
    {
        return View(new UserRegisterModel());
    }
    [HttpPost]
    [NonAuthorized]
    public async Task<IActionResult> Register(UserRegisterModel user)
    {
        if (!ModelState.IsValid)
        {
            return View(user);
        }
        var result = await _authService.RegisterAsync(user);
        if (result.IsFailure)
        {
            ModelState.TryAddModelError("Ошибка", result.Error.Title);
            foreach (var error in result.Error.Details)
            {
                ModelState.TryAddModelError(error.Key, error.Value);
            }
            return View(user);
        }

        var userEntity = result.Value;
        var sendResult = await SendConfirmCodeOnEmailAsync(userEntity);
        if (sendResult)
        {
            return View(nameof(ConfirmEmail), "Подтвердите вашу почту, вам направлено письмо, пройдите по ссылке. " +
                "Без данного подтверждения ваш аккаунт не будет активирован");
        }
        ViewBag.IsEmailFormNeeded = true;
        return View(nameof(ConfirmEmail), "Произошла ошибка при отправке письма с ссылкой для подтверждения аккаунта. " +
            "Пожалуйста, введите, привязанную почту ещё раз, мы вышлем новое письмо. Без данного подтверждения ваш аккаунт не будет активирован.");
    }

    [NonAuthorized]
    public async Task<IActionResult> ConfirmEmail(string userId, string code)
    {
        var result = await _authService.ConfirmEmailAsync(userId, code);
        if (result.IsFailure)
        {
            ViewBag.IsEmailFormNeeded = true;
            return View(nameof(ConfirmEmail), $"Произошла ошибка: {result.Error.Title}.{Environment.NewLine}" +
            "Пожалуйста, введите, привязанную почту ещё раз, мы вышлем новое письмо. Без данного подтверждения ваш аккаунт не будет активирован.");
        }
        return View(nameof(ConfirmEmail), "Подтверждение прошло успешно! Вы можете войти в аккаунт под своими данными.");
    }

    [NonAuthorized]
    public async Task<IActionResult> ResendEmail(UserEmailModel userMail)
    {
        var successMessage = "Подтвердите вашу почту, вам направлено письмо, пройдите по ссылке. " +
            "Без данного подтверждения ваш аккаунт не будет активирован.";
        if (string.IsNullOrEmpty(userMail.Email) || !ModelState.IsValid)
        {
            return View(nameof(ConfirmEmail), "Неправильно введённая почта. Пожалуйста, введите, привязанную почту ещё раз, мы вышлем новое письмо. " +
                "Без данного подтверждения ваш аккаунт не будет активирован.");
        }
        var user = await _authService.FindByEmailAsync(userMail.Email);
        if (user is null || user.EmailConfirmed)
        {
            return View(nameof(ConfirmEmail), successMessage);
        }
        var sendResult = await SendConfirmCodeOnEmailAsync(user);
        if (!sendResult)
        {
            ViewBag.IsEmailFormNeeded = true;
            return View(nameof(ConfirmEmail), "Произошла ошибка при отправке письма с ссылкой для подтверждения аккаунта. " +
                "Пожалуйста, введите, привязанную почту ещё раз, мы вышлем новое письмо. Без данного подтверждения ваш аккаунт не будет активирован.");
        }
        return View(nameof(ConfirmEmail), successMessage);
    }

    [NonAuthorized]
    public IActionResult Login(string returnUrl = null)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            TempData["returnUrl"] = returnUrl;
        }
        return View(new UserLoginModel());
    }

    [NonAuthorized]
    [HttpPost]
    public async Task<IActionResult> Login(UserLoginModel user)
    {
        if (!ModelState.IsValid)
        {
            return View(user);
        }
        var result = await _authService.LoginAsync(user);
        if (result.IsFailure)
        {
            if (result.Error == ErrorAuth.UnconfirmedEmail)
            {
                ViewBag.IsEmailFormNeeded = true;
                return View(nameof(ConfirmEmail), "Ваш аккаунт не был подтверждён. " +
                    "Пожалуйста, введите, привязанную почту ещё раз, мы вышлем новое письмо. Без данного подтверждения ваш аккаунт не будет активирован.");
            }
            ModelState.AddModelError("", "Неправильное имя или пароль");
            return View(user);
        }
        var returnUrl = TempData["returnUrl"]?.ToString();
        if (!string.IsNullOrEmpty(returnUrl))
        {
            return Redirect(returnUrl);
        }
        return RedirectToAction(nameof(HomeController.Index), ControllerExt.NameOf<HomeController>());
    }

    [NonAuthorized]
    public IActionResult ForgotPassword()
    {
        return View(new UserEmailModel());
    }

    [NonAuthorized]
    [HttpPost]
    public async Task<IActionResult> ForgotPassword(UserEmailModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        var message = "Письмо с инструкцией по сбросу пароля было направлено на вашу почту.";
        var result = await _authService.GenerateForgotPasswordTokenAsync(model);
        if (result.IsFailure)
        {
            return View(nameof(ConfirmEmail), message);
        }
        var user = await _authService.FindByEmailAsync(model.Email);
        var callbackUrl = Url.Action(nameof(ResetPassword), ControllerExt.NameOf<AccountController>(),
                                    new { userId = user.Id, token = result.Value }, protocol: HttpContext.Request.Scheme);

        var sendResult = await _authService.SendEmailAsync(user.Email, "Сброс пароля вашего аккаунта",
                        $"Вы начали процедуру сброса пароля для своего аккаунта в нашем магазине TestAppMarket. Пройдите по ссылке: <a href='{callbackUrl}'>link</a>. " +
                        $"После перехода по ссылке ваш пароль сбросится и вы сможете ввести новый.{Environment.NewLine}{Environment.NewLine}" +
                        $"{Environment.NewLine}Если вы не сбрасывали пароль аккаунта в нашем магазине, то игнорируйте это письмо и не переходите по ссылке.");
        if (!sendResult)
        {
            return View(nameof(ConfirmEmail), "Упс, произошла ошибка! Попробуйте повторить процедуру с самого начала.");
        }
        return View(nameof(ConfirmEmail), message);
    }

    [NonAuthorized]
    public async Task<IActionResult> ResetPassword(string userId, string token)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
        {
            return RedirectToAction(nameof(Login));
        }
        return View(new UserResetPasswordModel
        {
            Id = userId,
            Token = token
        });
    }

    [NonAuthorized]
    [HttpPost]
    public async Task<IActionResult> ResetPassword(UserResetPasswordModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        var result = await _authService.ResetPasswordAsync(model);
        if (result.IsFailure)
        {
            foreach (var error in result.Error.Details)
            {
                ModelState.TryAddModelError(error.Key, error.Value);
            }
            return View(model);
        }
        return View(nameof(ConfirmEmail), "Пароль успешно изменён! Теперь вы можете войти под новыми данными.");
    }

    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await _authService.LogoutAsync();
        return RedirectToAction(nameof(HomeController.Index), ControllerExt.NameOf<HomeController>());
    }

    public IActionResult AccessDenied()
    {
        return View();
    }

    private async Task<bool> SendConfirmCodeOnEmailAsync(MarketUser userEntity)
    {
        var code = await _authService.GenerateEmailConfirmationTokenAsync(userEntity);
        var callbackUrl = Url.Action(nameof(ConfirmEmail), ControllerExt.NameOf<AccountController>(),
                                    new { userId = userEntity.Id, code = code }, protocol: HttpContext.Request.Scheme);
        var messageTitle = "Подтвердите ваш аккаунт";
        var messageBody = $"Подтвердите регистрацию в нашем магазине TestAppMarket, перейдя по ссылке: <a href='{callbackUrl}'>link</a>. " +
                        $"Без данного подтверждения ваш аккаунт не будет активирован.{Environment.NewLine}{Environment.NewLine}" +
                        $"{Environment.NewLine}Если вы не создавали аккаунт в нашем магазине, то игнорируйте это письмо и не переходите по ссылке.";
        return await _authService.SendEmailAsync(userEntity.Email, messageTitle, messageBody);
    }

    private void AddTempDataForModelStateErrors()
    {
        var dict = new Dictionary<string, string>();
        foreach (var error in ModelState)
        {
            if (error.Value.ValidationState == ModelValidationState.Invalid)
            {
                dict.TryAdd(error.Key, error.Value.Errors[0].ErrorMessage);
            }
        }
        TempData[_modelStateForTempData] = JsonSerializer.Serialize(dict);
    }

    private void AddTempDataForNotification(string message, bool IsSuccessMessage)
    {
        TempData[ControllerExt.NotiNameForTempData] = message;
        if (!IsSuccessMessage) { TempData[ControllerExt.ErrorNameForTempData] = true; }
    }
}
