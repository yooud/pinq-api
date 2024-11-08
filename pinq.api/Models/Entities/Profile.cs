namespace pinq.api.Models.Entities;

public class Profile
{
    public int UserId { get; set; }

    public string Username { get; set; }

    public string DisplayName { get; set; }

    public string Status { get; set; }

    public int PhotoId { get; set; }

    public int BatteryStatus { get; set; }
}