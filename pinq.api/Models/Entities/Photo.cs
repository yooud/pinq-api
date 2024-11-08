namespace pinq.api.Models.Entities;

public class Photo
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public Type PhotoType { get; set; }
    
    public string ImageCode { get; set; }

    public string ImageUrl { get; set; }
    
    public DateTime CreatedAt { get; set; }

    public enum Type
    {
        Avatar,
        Post,
        Chat
    }
}