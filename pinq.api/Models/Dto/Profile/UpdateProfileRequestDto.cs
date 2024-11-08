using System.ComponentModel.DataAnnotations;
using pinq.api.Filters;

namespace pinq.api.Models.Dto.Profile;

[AtLeastOneRequired(nameof(Username), nameof(DisplayName))]
public class UpdateProfileRequestDto
{
    [Length(5,20)]
    public string? Username { get; set; }

    [Length(5,20)]
    // TODO: https://mgorbatyuk.dev/blog/development/2021-02-20-snake-case-and-asp-net-core/
    public string? DisplayName { get; set; }
}