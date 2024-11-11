using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pinq.api.Filters;
using pinq.api.Models.Entities;
using pinq.api.Repository;
using pinq.api.Services;

namespace pinq.api.Controllers;

[ApiController]
[Route("photo")]
[ValidateSession]
[Authorize]
public class PhotoController(IPhotoRepository photoRepository,
    IUserRepository userRepository,
    IStorageService storageService) : ControllerBase
{
    [HttpPost]
    [Route("{type}")]
    public async Task<IActionResult> UploadFile([FromForm] IFormFile? file, string type)
    {
        var photoType = type switch
        {
            "profile" => "avatar",
            "post" => "post",
            "chat" => "chat",
            _ => null
        };

        if (photoType is null)
            return BadRequest(new { Message = "Invalid photo type" });

        var uid = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        if (file is null || file.Length == 0)
            return BadRequest(new { Message = "File is empty." });

        var photoCode = Guid.NewGuid().ToString();

        await using var stream = file.OpenReadStream();
        var imageUrl = await storageService.UploadFileAsync(stream, photoCode);
        
        var user = await userRepository.GetUserByUid(uid);
        var photo = new Photo
        {
            UserId = user.Id,
            ImageCode = photoCode,
            ImageUrl = imageUrl,
            PhotoType = photoType
        };
        await photoRepository.CreatePhotoAsync(photo);

        return Ok(new { Id = photoCode });
    }
}