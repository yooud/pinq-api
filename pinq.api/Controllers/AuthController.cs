using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pinq.api.Filters;
using pinq.api.Models.Dto.Auth;
using pinq.api.Repository;
using pinq.api.Services;

namespace pinq.api.Controllers;

[ApiController]
[Route("auth")]
[Authorize]
public class AuthController(
    IUserRepository userRepository,
    IUserProfileRepository userProfileRepository,
    IUserSessionRepository sessionRepository,
    ISessionCacheService sessionCacheService) : ControllerBase
{
    [HttpGet]
    [ValidateSession]
    public async Task<IActionResult> GetProfileStatus()
    {
        var uid = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var isExists = await userProfileRepository.IsExists(uid);
        return Ok(new { is_profile_complete = isExists });
    }
    
    [HttpPost]
    public async Task<IActionResult> Login([FromBody] LoginDto request)
    {
        var uid = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        if (!await userRepository.IsExistsByUid(uid))
        {
            var email = User.FindFirstValue(ClaimTypes.Email)!;
            if (await userRepository.IsExistsByEmail(email))
                return BadRequest(new { Message = "Email already exists" });
            await userRepository.CreateUser(uid, email);
        }

        var sessionId = Guid.NewGuid();
        await sessionRepository.UpdateSession(uid, request.FcmToken, sessionId);
        await sessionCacheService.SetSessionAsync(uid, sessionId.ToString());
        
        Response.Headers["X-Session-Id"] = sessionId.ToString();
        return NoContent();
    }

    [HttpDelete]
    [ValidateSession]
    public async Task<IActionResult> Logout()
    {
        var uid = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        
        await sessionRepository.DeleteSession(uid);
        await sessionCacheService.InvalidateSessionAsync(uid);

        return NoContent();
    }
}