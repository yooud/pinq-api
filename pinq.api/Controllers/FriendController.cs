using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using pinq.api.Filters;
using pinq.api.Models.Dto;
using pinq.api.Models.Dto.Profile;
using pinq.api.Models.Entities;
using pinq.api.Repository;

namespace pinq.api.Controllers;

[ApiController]
[Route("friends")]
[Authorize]
[ValidateSession]
public class FriendController(
    IFriendRepository friendRepository,
    IFriendRequestRepository friendRequestRepository,
    IUserProfileRepository profileRepository) : ControllerBase
{
    [HttpPost("{username}")]
    public async Task<IActionResult> SendRequest(string username)
    {
        var uid = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var sender = await profileRepository.GetProfileByUid(uid);
        if (sender is null) 
            return BadRequest(new { message = "You have not complete your profile." });
        
        var receiver = await profileRepository.GetProfileByUsername(username);
        if (receiver is null)
            return NotFound(new { Message = "User not found" });
        
        var isFriends = await friendRepository.IsFriendsAsync(receiver.UserId, sender.UserId);
        if (isFriends) 
            return BadRequest(new { message = "You are already friends" });
        
        var friendRequest = await friendRequestRepository.GetFriendRequestAsync(receiver.UserId, sender.UserId);
        if (friendRequest is null)
        {
            friendRequest = new FriendRequest()
            {
                SenderId = sender.UserId,
                ReceiverId = receiver.UserId
            };
            friendRequest = await friendRequestRepository.CreateFriendRequestAsync(friendRequest);
        }
        else
        {
            friendRequest = await friendRequestRepository.AcceptFriendRequestAsync(friendRequest.Id);
            await friendRepository.CreateFriendshipAsync(receiver.UserId, sender.UserId);
        }
        return Ok(new { Status = friendRequest.Status });
    }

    [HttpGet]
    public async Task<IActionResult> GetFriendRequests(
        [FromQuery][Range(0, int.MaxValue, ErrorMessage = "The skip parameter must be a non-negative integer.")] int skip = 0,
        [FromQuery][Range(0, int.MaxValue, ErrorMessage = "The count parameter must be a non-negative integer.")] int count = 50,
        [FromQuery] string type = "incoming"
    )
    {
        var uid = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        
        var allowedTypes = new[] { "incoming", "outgoing" };
        if (!allowedTypes.Contains(type.ToLower()))
            return BadRequest(new { Message = "Invalid type. Allowed values: incoming, outgoing." });
        
        var profiles = await friendRequestRepository.GetFriendRequestsAsync(uid, type, count, skip);
        var totalCount = await friendRequestRepository.CountFriendRequestsAsync(uid, type);
        
        return Ok(new PaginatedListDto
        {
            Data = profiles.Select(profile => new ProfileDto
            {
                Username = profile.Username,
                DisplayName = profile.DisplayName,
                ProfilePictureUrl = profile.Photo?.ImageUrl
            }),
            Pagination = new PaginatedListDto.Metadata
            {
                Skip = skip,
                Count = profiles.Count(),
                Total = totalCount
            }
        });
    }

    [HttpDelete("{username}")]
    public async Task<IActionResult> DeleteFriend(string username)
    {
        var uid = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var sender = await profileRepository.GetProfileByUid(uid);
        if (sender is null) 
            return BadRequest(new { message = "You have not complete your profile." });
        
        var receiver = await profileRepository.GetProfileByUsername(username);
        if (receiver is null)
            return NotFound(new { Message = "User not found" });
        
        var isFriends = await friendRepository.IsFriendsAsync(receiver.UserId, sender.UserId);
        if (isFriends)
        {
            await friendRepository.DeleteFriendshipAsync(receiver.UserId, sender.UserId);
            return Ok();
        }
        
        var friendRequest = await friendRequestRepository.GetFriendRequestAsync(receiver.UserId, sender.UserId);
        if (friendRequest is null)
            return BadRequest(new { message = "You are not friends and there are no friend requests." });
        
        if (friendRequest.SenderId == sender.UserId)
            friendRequest = await friendRequestRepository.CancelFriendRequestAsync(friendRequest.Id);
        else 
            friendRequest = await friendRequestRepository.RejectFriendRequestAsync(friendRequest.Id);

        return Ok(new { Status = friendRequest.Status });
    }
}