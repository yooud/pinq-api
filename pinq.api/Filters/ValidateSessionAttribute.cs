using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using pinq.api.Services;

namespace pinq.api.Filters;

public class ValidateSessionAttribute : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var sessionCacheService = context.HttpContext.RequestServices.GetService<ISessionCacheService>();
        
        var userId = context.HttpContext.User.FindFirst("user_id")!.Value;
        var sessionId = context.HttpContext.Request.Headers["X-Session-Id"].ToString();

        if (string.IsNullOrWhiteSpace(sessionId))
        {
            context.HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.HttpContext.Response.ContentType = "application/json";
            await context.HttpContext.Response.WriteAsync(JsonConvert.SerializeObject(new { Message = "X-Session-Id is empty." }));
            return;
        }
        
        var isValid = await sessionCacheService.ValidateSessionAsync(userId, sessionId);
        if (!isValid)
        {
            context.HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.HttpContext.Response.ContentType = "application/json";
            await context.HttpContext.Response.WriteAsync(JsonConvert.SerializeObject(new { Message = "Invalid session." }));
            return;
        }
        
        await next();
    }
}