namespace pinq.api.Models.Dto.Admin;

public class UpdateUserRequestDto
{
    public UserRole Role { get; set; }
    
    public enum UserRole
    {
        User,
        Moderator,
        Admin
    }
}