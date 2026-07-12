using System.ComponentModel.DataAnnotations;

namespace SufiChain.SufiPlatform.Account;

public class VerifyPasswordResetTokenInput
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public string ResetToken { get; set; } = string.Empty;
}
