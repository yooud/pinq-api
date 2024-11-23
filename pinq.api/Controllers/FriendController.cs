using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using pinq.api.Filters;
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
}