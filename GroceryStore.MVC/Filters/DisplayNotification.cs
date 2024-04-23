using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using GroceryStore.MVC.Extensions;
using GroceryStore.MVC.Models;

namespace GroceryStore.MVC.Filters;

public class DisplayNotification : ActionFilterAttribute
{
    public override void OnActionExecuted(ActionExecutedContext context)
    {
        var controller = context.Controller as Controller;
        var tempdata = controller.TempData;
        var viewbag = controller.ViewBag;
        if (tempdata[ControllerExt.NotiNameForTempData] is not null)
        {
            if (tempdata[ControllerExt.ErrorNameForTempData] is not null)
            {
                viewbag.Notification = new NotificationVM(tempdata[ControllerExt.NotiNameForTempData].ToString(), true);
            }
            else
            {
                viewbag.Notification = new NotificationVM(tempdata[ControllerExt.NotiNameForTempData].ToString());
            }
        }
    }
}
