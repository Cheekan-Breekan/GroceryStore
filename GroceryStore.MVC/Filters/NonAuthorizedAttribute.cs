using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using GroceryStore.MVC.Extensions;
using GroceryStore.MVC.Controllers;

namespace GroceryStore.MVC.Filters;

public class NonAuthorizedAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.HttpContext.User.Identity.IsAuthenticated)
        {
            context.Result = new RedirectToActionResult(nameof(AccountController.Profile), ControllerExt.NameOf<AccountController>(), null);
        }
    }
}
