using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pinq.api.Filters;
using pinq.api.Models.Dto;
using pinq.api.Repository;

namespace pinq.api.Controllers;

[ApiController]
[Route("chat")]
[Authorize]
[ValidateSession]
public class ChatController(
    IUserProfileRepository profileRepository,
    IChatRepository chatRepository) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetChats(
        [FromQuery][Range(0, int.MaxValue, ErrorMessage = "The skip parameter must be a non-negative integer.")] int skip = 0,
        [FromQuery][Range(0, int.MaxValue, ErrorMessage = "The count parameter must be a non-negative integer.")] int count = 10
    )
    {
        var uid = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var user = await profileRepository.GetProfileByUid(uid);
        if (user is null) 
            return BadRequest(new { message = "You have not complete your profile." });

        var chats = await chatRepository.GetChatsByUserIdAsync(user.UserId, count, skip);
        var totalCount = await chatRepository.CountChatsByUserIdAsync(user.UserId);
        
        return Ok(new PaginatedListDto
        {
            Data = chats,
            Pagination = new PaginatedListDto.Metadata
            {
                Skip = skip,
                Count = chats.Count,
                Total = totalCount,
            }
        });
    }
}