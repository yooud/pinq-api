using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pinq.api.Filters;
using pinq.api.Models.Dto;
using pinq.api.Models.Dto.Chat;
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
    
    [HttpGet("{username}/messages")]
    public async Task<IActionResult> GetChatMessages(
        string username, 
        [FromQuery][Range(0, int.MaxValue, ErrorMessage = "The skip parameter must be a non-negative integer.")] int skip = 0,
        [FromQuery][Range(0, int.MaxValue, ErrorMessage = "The count parameter must be a non-negative integer.")] int count = 10
    )
    {
        var uid = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var user = await profileRepository.GetProfileByUid(uid);
        if (user is null) 
            return BadRequest(new { message = "You have not complete your profile." });
        
        var chat = await chatRepository.GetChatByUsernamesAsync(user.Username, username);
        if (chat is null)
            return NotFound(new { message = "Chat not found." });
        
        var messages = await chatRepository.GetChatMessagesAsync(chat.Id, count, skip);
        var totalCount = await chatRepository.CountChatMessagesAsync(chat.Id);
        
        return Ok(new PaginatedListDto
        {
            Data = messages,
            Pagination = new PaginatedListDto.Metadata
            {
                Skip = skip,
                Count = messages.Count,
                Total = totalCount,
            }
        });
    }
    
    [HttpPost("{username}/messages")]
    public async Task<IActionResult> SendMessage(
        string username,
        [FromBody] SendMessageRequestDto request
    )
    {
        var uid = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var user = await profileRepository.GetProfileByUid(uid);
        if (user is null) 
            return BadRequest(new { message = "You have not complete your profile." });
        
        var chat = await chatRepository.GetChatByUsernamesAsync(user.Username, username);
        if (chat is null)
            return NotFound(new { message = "Chat not found." });
        
        var message = await chatRepository.SendMessageAsync(chat.Id, user.UserId, request);
        return StatusCode(201, message);
    }

    [HttpGet("{username}/messages/updates")]
    public async Task<IActionResult> GetChatUpdates(string username, [FromQuery] long lastUpdate)
    {
        var uid = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var user = await profileRepository.GetProfileByUid(uid);
        if (user is null)
            return BadRequest(new { message = "You have not complete your profile." });

        var chat = await chatRepository.GetChatByUsernamesAsync(user.Username, username);
        if (chat is null)
            return NotFound(new { message = "Chat not found." });

        var messages = await chatRepository.GetChatMessagesUpdatesAsync(chat.Id, lastUpdate);
        return Ok(messages);
    }
    
    [HttpDelete("{username}/messages/{messageId:int}")]
    public async Task<IActionResult> DeleteMessage(string username, int messageId)
    {
        var uid = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var user = await profileRepository.GetProfileByUid(uid);
        if (user is null) 
            return BadRequest(new { message = "You have not complete your profile." });
        
        var chat = await chatRepository.GetChatByUsernamesAsync(user.Username, username);
        if (chat is null)
            return NotFound(new { message = "Chat not found." });
        
        var message = await chatRepository.GetChatMessageByIdAsync(messageId);
        if (message is null)
            return NotFound(new { message = "Message not found." });
        
        if (message.ChatId != chat.Id)
            return BadRequest(new { message = "Message does not belong to this chat." });
        
        if (message.SenderId != user.UserId)
            return Unauthorized(new { message = "You are not authorized to delete this message." });
        
        await chatRepository.DeleteChatMessageAsync(messageId);
        return NoContent();
    }
}