using Microsoft.AspNetCore.Mvc;

namespace GroceryStore.MVC.Extensions;

public static class ControllerExt
{
    public const string NotiNameForTempData = "ShowNotification";
    public const string ErrorNameForTempData = "IsError";
    public static string NameOf<T>() where T : Controller
    {
        return typeof(T).Name.Replace("Controller", string.Empty);
    }
}
