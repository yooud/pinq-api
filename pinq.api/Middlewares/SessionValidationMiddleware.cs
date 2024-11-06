using Newtonsoft.Json;
using pinq.api.Services;

namespace pinq.api.Middlewares;

public class SessionValidationMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ISessionCacheService cacheService)
    {
        var userId = context.User.FindFirst("user_id")!.Value;
        var sessionId = context.Request.Headers["X-Session-Id"].ToString();

        if (string.IsNullOrWhiteSpace(sessionId))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonConvert.SerializeObject(new { Message = "X-Session-Id is empty." }));
            return;
        }
        
        var isValid = await cacheService.ValidateSessionAsync(userId, sessionId);
        if (!isValid)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonConvert.SerializeObject(new { Message = "Invalid session." }));
            return;
        }
        
        await next(context);
    }
}
