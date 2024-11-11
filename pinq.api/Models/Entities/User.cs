namespace pinq.api.Models.Entities;

public class User
{
    public int Id { get; set; }

    public string Uid { get; set; }

    public string Email { get; set; }

    public bool IsBanned { get; set; }

    public DateTime BannedAt { get; set; }

    public DateTime CreatedAt { get; set; }
}