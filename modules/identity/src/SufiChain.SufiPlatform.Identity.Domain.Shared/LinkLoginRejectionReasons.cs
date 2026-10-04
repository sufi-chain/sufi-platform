namespace SufiChain.SufiPlatform.Identity;

/// <summary>
/// Stable reasons for a rejected linked-account login.
/// These values are log fields. They are not tokens and they are not secrets.
/// </summary>
public static class LinkLoginRejectionReasons
{
    public const string MissingToken = "MissingToken";
    public const string SourceUserNotFound = "SourceUserNotFound";
    public const string TokenInvalid = "TokenInvalid";
    public const string TokenExpired = "TokenExpired";
    public const string TokenPurposeMismatch = "TokenPurposeMismatch";
    public const string TokenUserMismatch = "TokenUserMismatch";
    public const string TokenSecurityStampMismatch = "TokenSecurityStampMismatch";
    public const string NotLinked = "NotLinked";
    public const string TenantMismatch = "TenantMismatch";
    public const string TargetUserNotFound = "TargetUserNotFound";
}
