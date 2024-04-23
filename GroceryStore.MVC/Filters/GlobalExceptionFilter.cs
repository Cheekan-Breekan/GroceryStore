using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace GroceryStore.MVC.Filters;

public class GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger, IModelMetadataProvider meta) : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (!context.HttpContext.Request.Headers["X-Requested-With"].Equals("XMLHttpRequest"))
        {
            return;
        }
        logger.LogError(context.Exception, context.Exception.Message);
        var error = new ProblemDetails
        {
            Title = ReasonPhrases.GetReasonPhrase(500),
            Status = StatusCodes.Status500InternalServerError,
            Detail = "Произошла внутренняя ошибка при обработке вашего запроса. Приносим свои извинения."
        };
        var result = new ViewResult
        {
            ViewData = new ViewDataDictionary(meta, context.ModelState)
            {
                Model = error
            },
            ViewName = "~/Views/Error/Index.cshtml"
        };

        context.ExceptionHandled = true;
        context.Result = result;
    }
}
