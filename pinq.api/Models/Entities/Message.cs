namespace pinq.api.Models.Entities;

public class Message
{
    public int Id { get; set; }

    public int ChatId { get; set; }
    
    public int SenderId { get; set; }

    public object Content { get; set; }
    
    public DateTime SentAt { get; set; }

    public DateTime? EditedAt { get; set; }
    
    public DateTime? SeenAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
}