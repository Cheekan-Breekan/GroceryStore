using Microsoft.AspNetCore.Mvc;

namespace GroceryStore.Api.Middlewares;

public class GlobalExceptionHandlerMiddleware : IMiddleware
{
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    public GlobalExceptionHandlerMiddleware(ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _logger = logger;
    }
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            var details = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Type = "https://tools.ietf.org/html/rfc9110#section-15.6.1", //ссылка на описание 500 ошибки
                Title = "An internal server error has occured",
                Detail = ex.Message
            };
            await context.Response.WriteAsJsonAsync(details);
        }
    }
}
