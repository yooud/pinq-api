using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pinq.api.Models.Dto.Auth;
using pinq.api.Repository;
using pinq.api.Services;

namespace pinq.api.Controllers;

[ApiController]
[Route("auth")]
[Authorize]
public class AuthController(
    IUserRepository userRepository,
    IUserSessionRepository sessionRepository,
    ISessionCacheService sessionCacheService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Login([FromBody] LoginDto request)
    {
        var uid = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        if (!await userRepository.IsExists(uid))
        {
            var email = User.FindFirstValue(ClaimTypes.Email)!;
            await userRepository.CreateUser(uid, email);
        }

        var sessionId = Guid.NewGuid();
        await sessionRepository.UpdateSession(uid, request.FcmToken, sessionId);
        await sessionCacheService.SetSessionAsync(uid, sessionId.ToString());
        
        Response.Headers["X-Session-Id"] = sessionId.ToString();
        return NoContent();
    }
}