namespace SufiChain.SufiPlatform.Identity;

/// <summary>
/// Constants for Identity Security Log Identity field values.
/// </summary>
public static class IdentitySecurityLogIdentityConsts
{
    /// <summary>
    /// Identity provider type (e.g., "Identity" for local authentication).
    /// </summary>
    public const string Identity = "Identity";
    
    /// <summary>
    /// External provider type (e.g., "Google", "Facebook", etc.).
    /// </summary>
    public const string External = "External";

    public const string IdentityExternal = "IdentityExternal";

    public const string IdentityTwoFactor = "IdentityTwoFactor";
}

public class IdentitySecurityLogConsts
{
    public static int MaxApplicationNameLength { get; set; } = 96;
    public static int MaxIdentityLength { get; set; } = 96;
    public static int MaxActionLength { get; set; } = 96;
    public static int MaxUserNameLength { get; set; } = 256;
    public static int MaxTenantNameLength { get; set; } = 64;
    public static int MaxClientIpAddressLength { get; set; } = 64;
    public static int MaxClientIdLength { get; set; } = 64;
    public static int MaxCorrelationIdLength { get; set; } = 64;
    public static int MaxBrowserInfoLength { get; set; } = 512;
}

/// <summary>
/// Constants for Identity Security Log Action field values.
/// </summary>
public static class IdentitySecurityLogActionConsts
{
    public const string LoginSucceeded = "LoginSucceeded";
    public const string LoginFailed = "LoginFailed";
    public const string LoginInvalidUserName = "LoginInvalidUserName";
    public const string LoginInvalidUserNameOrPassword = "LoginInvalidUserNameOrPassword";
    public const string LoginLockedout = "LoginLockedout";
    public const string LoginRequiresTwoFactor = "LoginRequiresTwoFactor";
    public const string LoginNotAllowed = "LoginNotAllowed";
    public const string Logout = "Logout";
    public const string Register = "Register";
    public const string ChangePassword = "ChangePassword";
    public const string ChangeEmail = "ChangeEmail";
    public const string ChangePhoneNumber = "ChangePhoneNumber";
    public const string TwoFactorEnabled = "TwoFactorEnabled";
    public const string TwoFactorDisabled = "TwoFactorDisabled";
}
