using System.ComponentModel.DataAnnotations;

namespace SufiChain.SufiAbp.Account;

public class ResetPasswordDto
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public string ResetToken { get; set; } = string.Empty;

    [Required]
    [StringLength(128)]
    public string Password { get; set; } = string.Empty;
}
