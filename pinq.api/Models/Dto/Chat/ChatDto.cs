namespace pinq.api.Models.Dto.Chat;

public class ChatDto
{
    public ChatProfileDto Profile { get; set; }
    
    public MessageDto LastMessage { get; set; }
}