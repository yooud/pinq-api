using System.ComponentModel.DataAnnotations;

namespace pinq.api.Models.Dto.Auth;

public class LoginDto
{
    [Required]
    public string FcmToken { get; set; }
}