using System;
using System.ComponentModel.DataAnnotations;

namespace SufiChain.SufiPlatform.Account;

public class ConfirmEmailDto
{
    [Required]
    public Guid UserId { get; set; }

    [Required]
    public string ConfirmationToken { get; set; } = string.Empty;
}
