using System.ComponentModel.DataAnnotations;

namespace pinq.api.Models.Dto.Chat;

public class SendMessageRequestDto
{
    [MinLength(1)]
    public string Text { get; set; }
}