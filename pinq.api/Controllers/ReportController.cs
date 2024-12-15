using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using pinq.api.Filters;
using pinq.api.Models.Dto.Report;
using pinq.api.Models.Entities;
using pinq.api.Repository;

namespace pinq.api.Controllers;

[ApiController]
[Route("report")]
[Authorize]
[ValidateSession]
public class ReportController(
    IUserProfileRepository profileRepository,
    IComplaintRepository complaintRepository) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateReport([FromBody] CreateReportRequestDto request)
    {
        var contentType = request.ContentType switch
        {
            CreateReportRequestDto.ReportContentType.ProfilePicture => "avatar",
            CreateReportRequestDto.ReportContentType.User => "user",
            CreateReportRequestDto.ReportContentType.Post => "post",
            _ => null
        };
        
        if (contentType is null)
            return BadRequest(new { Message = "Invalid content type" });
        
        var uid = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var sender = await profileRepository.GetProfileByUid(uid);
        if (sender is null) 
            return BadRequest(new { message = "You have not complete your profile." });

        var targetUser = await profileRepository.GetProfileByUsername(request.TargetUsername);
        if (targetUser is null)
            return NotFound(new { message = "User not found" });
        
        // TODO: check post 
        
        var complaint = new Complaint
        {
            UserId = sender.UserId,
            TargetUserId = targetUser.UserId,
            ContentType = contentType,
            ContentId = request.PostId,
            Reason = request.Reason
        };
        complaint = await complaintRepository.CreateComplaintAsync(complaint);
        
        if (complaint.Id == 0)
            return BadRequest(new { message = "Failed to create complaint" });
        
        return NoContent();
    }
}