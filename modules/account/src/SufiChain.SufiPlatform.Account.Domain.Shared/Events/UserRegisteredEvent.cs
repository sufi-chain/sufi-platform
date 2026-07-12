using System;

namespace SufiChain.SufiPlatform.Account;

public class UserRegisteredEvent
{
    public Guid UserId { get; set; }

    public string Email { get; set; } = string.Empty;

    public string AppName { get; set; } = string.Empty;

    public string? EmailConfirmationToken { get; set; }

    public string? ReturnUrl { get; set; }

    public string? ReturnUrlHash { get; set; }
}
