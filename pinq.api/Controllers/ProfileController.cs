using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pinq.api.Filters;
using pinq.api.Models.Dto;
using pinq.api.Models.Dto.Profile;
using pinq.api.Models.Entities;
using pinq.api.Repository;

namespace pinq.api.Controllers;

[ApiController]
[Route("profile")]
[ValidateSession]
[Authorize]
public class ProfileController(
    IUserProfileRepository profileRepository,
    IFriendRepository friendRepository,
    IPhotoRepository photoRepository) : ControllerBase
{
    [HttpGet("{username}")]
    public async Task<IActionResult> GetProfile(string username)
    {
        var uid = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        
        var myProfile = await profileRepository.GetProfileByUid(uid);
        if (myProfile is null)
            return NotFound(new { Message = "You have not complete your profile" });

        Photo? photo;
        if (username == "me")
        {
            photo = await photoRepository.GetPhotoByIdAsync(myProfile.PhotoId);
            return Ok(new ProfileDto
            {
                Username = myProfile.Username,
                DisplayName = myProfile.DisplayName,
                ProfilePictureUrl = photo?.ImageUrl
            });
        }
        
        var userProfile = await profileRepository.GetProfileByUsername(username);
        if (userProfile is null)
            return NotFound(new { Message = "Profile not found" });
        
        var isFriends = await friendRepository.IsFriendsAsync(myProfile.UserId, userProfile.UserId);
        photo = await photoRepository.GetPhotoByIdAsync(userProfile.PhotoId);
        return Ok(new ProfileDto
        {
            Username = userProfile.Username,
            DisplayName = userProfile.DisplayName,
            ProfilePictureUrl = photo?.ImageUrl,
            IsFriend = isFriends
        });
    }
    
    [HttpPatch]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequestDto request)
    {
        var uid = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        if (request.Username is not null)
        {
            var isUsernameTaken = await profileRepository.IsUsernameTaken(request.Username);
            if (isUsernameTaken)
            {
                return BadRequest(new { Message = "Username already taken" });
            }
        }

        Photo? photo = null;
        if (request.PictureId is not null)
        {
            var isPhotoAccessed = await photoRepository.IsPhotoCanBeAccessed(uid, request.PictureId);
            if (!isPhotoAccessed) 
                return BadRequest(new { Message = "Wrong photo id" });
            photo = await photoRepository.GetPhotoByCode(request.PictureId);
        }

        Profile profile;
        var isExists = await profileRepository.IsExists(uid);
        if (isExists)
        {
            profile = await profileRepository.UpdateProfileAsync(uid, request.Username, request.DisplayName, photo?.Id);
        }
        else
        {
            if (request.Username is not null && request.DisplayName is not null)
                profile = await profileRepository.CreateProfileAsync(uid, request.Username, request.DisplayName);
            else 
                return BadRequest(new { Message = "Username and display name is required" });
        }
        
        photo ??= await photoRepository.GetPhotoByIdAsync(profile.PhotoId);
        return Ok(new ProfileDto
        {
            Username = profile.Username,
            DisplayName = profile.DisplayName,
            ProfilePictureUrl = photo?.ImageUrl
        });
    }
    
    [HttpGet("{username}/friends")]
    public async Task<IActionResult> GetFriends(string username,
        [FromQuery][Range(0, int.MaxValue, ErrorMessage = "The skip parameter must be a non-negative integer.")] int skip = 0,
        [FromQuery][Range(0, int.MaxValue, ErrorMessage = "The count parameter must be a non-negative integer.")] int count = 50
    )
    {
        var uid = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        
        var myProfile = await profileRepository.GetProfileByUid(uid);
        if (myProfile is null)
            return NotFound(new { Message = "You have not complete your profile" });
        
        var myFriendsCount = await friendRepository.CountFriendsAsync(myProfile.UserId);
        IEnumerable<Profile>? profiles;
        Profile[]? enumerable;
        if (username == "me")
        {
            profiles = await friendRepository.GetFriendsAsync(myProfile.UserId, count, skip);
            enumerable = profiles as Profile[] ?? profiles.ToArray();
            return Ok(new PaginatedListDto
            {
                Data = enumerable.Select(profile => new ProfileDto
                {
                    Username = profile.Username,
                    DisplayName = profile.DisplayName,
                    ProfilePictureUrl = profile.Photo?.ImageUrl,
                    IsFriend = true
                }),
                Pagination = new PaginatedListDto.Metadata
                {
                    Skip = skip,
                    Count = enumerable.Length,
                    Total = myFriendsCount
                }
            });
        }
        
        var userProfile = await profileRepository.GetProfileByUsername(username);
        if (userProfile is null)
            return NotFound(new { Message = "Profile not found" });
        
        var myFriendsProfiles = await friendRepository.GetFriendsAsync(myProfile.UserId, myFriendsCount, 0);
       
        profiles = await friendRepository.GetFriendsAsync(userProfile.UserId, count, skip);
        var totalCount = await friendRepository.CountFriendsAsync(userProfile.UserId);

        enumerable = profiles as Profile[] ?? profiles.ToArray();
        return Ok(new PaginatedListDto
        {
            Data = enumerable.Select(profile => new ProfileDto
            {
                Username = profile.Username,
                DisplayName = profile.DisplayName,
                ProfilePictureUrl = profile.Photo?.ImageUrl,
                IsFriend = myFriendsProfiles.Any(p => p.Username.Equals(profile.Username))
            }),
            Pagination = new PaginatedListDto.Metadata
            {
                Skip = skip,
                Count = enumerable.Length,
                Total = totalCount
            }
        });
    }
}