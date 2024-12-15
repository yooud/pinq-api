using System.ComponentModel.DataAnnotations;
using pinq.api.Extensions;

namespace pinq.api.Models.Dto.Report;

[CustomValidation(typeof(CreateReportRequestDto), nameof(ValidatePostId))]
public class CreateReportRequestDto
{
    [Length(5,20)]
    public string TargetUsername { get; set; }

    [Length(5,100)]
    public string Reason { get; set; }

    public ReportContentType ContentType { get; set; }
    
    public int? PostId { get; set; }
    
    public enum ReportContentType
    {
        User,
        ProfilePicture,
        Post
    }
    
    public static ValidationResult ValidatePostId(CreateReportRequestDto dto, ValidationContext context)
    {
        if (dto is { ContentType: ReportContentType.Post, PostId: null })
            return new ValidationResult($"{nameof(PostId).ToSnakeCase()} must be specified if {nameof(ContentType).ToSnakeCase()} is post.",
                [nameof(PostId).ToSnakeCase()]);

        return ValidationResult.Success;
    }
}