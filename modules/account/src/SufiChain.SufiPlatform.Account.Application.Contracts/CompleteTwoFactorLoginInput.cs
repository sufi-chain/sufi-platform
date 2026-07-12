namespace SufiChain.SufiPlatform.Account;

public class CompleteTwoFactorLoginInput
{
    public string PendingToken { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// <see cref="TwoFactorProviderNames.Authenticator"/> or <see cref="TwoFactorProviderNames.Email"/>.
    /// </summary>
    public string Provider { get; set; } = TwoFactorProviderNames.Authenticator;

  /// <summary>
    /// Optional recovery code (alternative to <see cref="Code"/>).
    /// </summary>
    public string? RecoveryCode { get; set; }
}
