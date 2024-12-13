using pinq.api.Models.Dto.Map;

namespace pinq.api.Models.Dto.Profile;

public class ProfileDto
{
    public string? Username { get; set; }

    public string? DisplayName { get; set; }

    public string? ProfilePictureUrl { get; set; }

    public bool? IsFriend { get; set; }

    public LocationDto Location { get; set; }

    public string Status { get; set; }

    public long LastActivity { get; set; }
}