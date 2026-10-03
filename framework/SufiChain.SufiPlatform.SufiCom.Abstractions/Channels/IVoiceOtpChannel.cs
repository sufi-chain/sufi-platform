namespace SufiChain.SufiPlatform.SufiCom.Channels;

/// <summary>
/// Voice channel with a dedicated transactional OTP API. When the configured provider implements this
/// interface, every OTP call must go through <see cref="SendOtpAsync"/>.
/// </summary>
public interface IVoiceOtpChannel : IVoiceChannel
{
    /// <summary>
    /// Returns a non-null localization key when the OTP route cannot be used. Call after <see cref="IVoiceChannel.Configure"/>.
    /// </summary>
    string? GetOtpConfigurationError();

    Task<VoiceCallDeliveryResult> SendOtpAsync(OtpMessage message);
}
