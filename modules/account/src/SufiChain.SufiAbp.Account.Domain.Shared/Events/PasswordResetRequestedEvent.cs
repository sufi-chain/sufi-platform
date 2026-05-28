using System;

namespace SufiChain.SufiAbp.Account;

public class PasswordResetRequestedEvent
{
    public Guid UserId { get; set; }

    public string Email { get; set; } = string.Empty;

    public string ResetToken { get; set; } = string.Empty;

    public string? AppName { get; set; }

    public string? ReturnUrl { get; set; }

    public string? ReturnUrlHash { get; set; }
}
