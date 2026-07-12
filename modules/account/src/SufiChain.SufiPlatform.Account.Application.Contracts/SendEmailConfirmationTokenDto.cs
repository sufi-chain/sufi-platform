using System;
using System.ComponentModel.DataAnnotations;

namespace SufiChain.SufiPlatform.Account;

public class SendEmailConfirmationTokenDto : CaptchaInputDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string? AppName { get; set; }

    public string? ReturnUrl { get; set; }

    public string? ReturnUrlHash { get; set; }
}
