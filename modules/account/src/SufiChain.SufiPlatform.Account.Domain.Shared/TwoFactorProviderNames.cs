namespace SufiChain.SufiPlatform.Account;

/// <summary>
/// ASP.NET Identity two-factor token provider names.
/// </summary>
public static class TwoFactorProviderNames
{
    public const string Authenticator = "Authenticator";

    public const string Email = "Email";

    /// <summary>
    /// ASP.NET Identity phone token provider (SMS and voice code delivery).
    /// </summary>
    public const string Phone = "Phone";
}
