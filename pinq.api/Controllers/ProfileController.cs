using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pinq.api.Filters;
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
    IPhotoRepository photoRepository) : ControllerBase
{
    [HttpGet("{username}")]
    public async Task<IActionResult> GetProfile(string username)
    {
        var uid = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        Profile? profile;
        if (username == "me")
            profile = await profileRepository.GetProfileByUid(uid);
        else 
            profile = await profileRepository.GetProfileByUsername(username);
        
        if (profile is null)
            return NotFound(new { Message = "Profile not found" });

        var photo = await photoRepository.GetPhotoByIdAsync(profile.PhotoId);
        return Ok(new ProfileDto
        {
            Username = profile.Username,
            DisplayName = profile.DisplayName,
            ProfilePictureUrl = photo?.ImageUrl
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
}