using System.ComponentModel.DataAnnotations;
using pinq.api.Filters;

namespace pinq.api.Models.Dto.Profile;

[AtLeastOneRequired(nameof(Username), nameof(DisplayName), nameof(PictureId))]
public class UpdateProfileRequestDto
{
    [Length(5,20)]
    public string? Username { get; set; }

    [Length(5,20)]
    
    public string? DisplayName { get; set; }
    
    [Length(36,36)]
    public string? PictureId { get; set; }
}