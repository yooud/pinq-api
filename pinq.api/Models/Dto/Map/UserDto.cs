namespace pinq.api.Models.Dto.Map;

public class UserDto
{
    public int Id { get; set; }
    
    public string Username { get; set; }
    
    public string DisplayName { get; set; }
    
    public string ProfilePicture { get; set; }
    
    public LocationDto Location { get; set; }
    
    public string Status { get; set; }
    
    public long LastActivity { get; set; }
}