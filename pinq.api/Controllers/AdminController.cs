using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pinq.api.Models.Dto;
using pinq.api.Models.Dto.Admin;
using pinq.api.Repository;
using IAuthorizationService = pinq.api.Services.IAuthorizationService;

namespace pinq.api.Controllers;

[ApiController]
[Route("admin")]
[Authorize]
public class AdminController(
    IUserProfileRepository profileRepository, 
    IAuthorizationService authorizationService) : ControllerBase
{
    [HttpGet("user")]
    public async Task<IActionResult> GetUsersList(
        [FromQuery][Range(0, int.MaxValue, ErrorMessage = "The skip parameter must be a non-negative integer.")] int skip = 0,
        [FromQuery][Range(0, int.MaxValue, ErrorMessage = "The count parameter must be a non-negative integer.")] int count = 50
    )
    {
        var uid = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        if (!await authorizationService.CheckUserRoleAsync(uid, "admin"))
            return Forbid();
        
        var users = await profileRepository.GetProfilesAsync(count, skip);
        
        foreach (var user in users) 
            user.Role = await authorizationService.GetUserRoleAsync(user.Uid);
        
        var totalCount = await profileRepository.CountProfilesAsync();
        
        return Ok(new PaginatedListDto
        {
            Data = users,
            Pagination = new PaginatedListDto.Metadata
            {
                Skip = skip,
                Count = users.Count(),
                Total = totalCount
            }
        });
    }
    
    [HttpPatch("user/{userUid}")]
    public async Task<IActionResult> ChangeUserRole(string userUid, [FromBody] UpdateUserRequestDto request)
    {
        var uid = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        if (!await authorizationService.CheckUserRoleAsync(uid, "admin"))
            return Forbid();
        
        var user = await profileRepository.GetProfileByUid(userUid);
        if (user is null)
            return NotFound(new { Message = "User not found" });
        
        var result = await authorizationService.SetUserRoleAsync(userUid, request.Role.ToString().ToLower());
        if (result)
            return NoContent();
        
        return StatusCode(500, new { message = "Failed to update user role" });
    }
}