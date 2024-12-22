namespace pinq.api.Models.Dto.Chat;

public class MessageDto
{
    public int Id { get; set; }
    
    public string SenderUsername { get; set; }

    public object Content { get; set; }
    
    public long SentAt { get; set; }

    public long? EditedAt { get; set; }
    
    public long? SeenAt { get; set; }
}