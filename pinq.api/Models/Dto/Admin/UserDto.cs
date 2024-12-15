using pinq.api.Models.Dto.Profile;

namespace pinq.api.Models.Dto.Admin;

public class UserDto
{
    public int Id { get; set; }

    public string Uid { get; set; }

    public string Email { get; set; }

    public bool IsBanned { get; set; }
    
    public ProfileDto Profile { get; set; }

    public string Role { get; set; }
    
    public long? BannedAt { get; set; }

    public long CreatedAt { get; set; }
}