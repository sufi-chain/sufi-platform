namespace SufiChain.SufiPlatform.SufiCom;

/// <summary>
/// One-time password message. Providers with a dedicated OTP API send <see cref="Code"/> through
/// their transactional template; free-text OTP routes send <see cref="Content"/>.
/// </summary>
public class OtpMessage
{
    public string Phone { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// One of <see cref="OtpPurposes"/>.
    /// </summary>
    public string Purpose { get; set; } = OtpPurposes.Verification;

    /// <summary>
    /// Fully rendered message text that contains <see cref="Code"/>.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    public string? AppName { get; set; }

    /// <summary>
    /// Stable key for one issued code. Providers that support request de-duplication use it so retries never send twice.
    /// </summary>
    public string IdempotencyKey { get; set; } = string.Empty;

    public string? Culture { get; set; }

    public Guid? TenantId { get; set; }
}
