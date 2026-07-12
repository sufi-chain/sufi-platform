using System;
using System.ComponentModel.DataAnnotations;

namespace SufiChain.SufiPlatform.Account;

public class VerifyEmailConfirmationTokenInput
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public string ConfirmationToken { get; set; } = string.Empty;
}
