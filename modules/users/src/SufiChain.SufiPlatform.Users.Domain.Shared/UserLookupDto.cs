using System;

namespace SufiChain.SufiPlatform.Users;

/// <summary>
/// Lightweight user projection for lookup and selector UI.
/// </summary>
public class UserLookupDto
{
    public Guid Id { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string? Name { get; set; }

    public string? Surname { get; set; }

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public bool IsActive { get; set; }

    public string DisplayName => string.IsNullOrWhiteSpace(Name) && string.IsNullOrWhiteSpace(Surname)
        ? UserName
        : $"{Name} {Surname}".Trim();
}
