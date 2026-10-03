namespace SufiChain.SufiPlatform.SufiCom.Channels;

/// <summary>
/// SMS channel with a dedicated transactional OTP API. When the configured provider implements this
/// interface, every OTP must go through <see cref="SendOtpAsync"/> and never through the plain send route.
/// </summary>
public interface ISmsOtpChannel : ISmsChannel
{
    /// <summary>
    /// Returns a non-null localization key when the OTP route cannot be used (for example, a missing template).
    /// Call after <see cref="ISmsChannel.Configure"/>.
    /// </summary>
    string? GetOtpConfigurationError();

    Task<SmsDeliveryResult> SendOtpAsync(OtpMessage message);
}
