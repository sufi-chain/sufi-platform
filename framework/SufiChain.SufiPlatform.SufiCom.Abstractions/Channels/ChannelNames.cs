namespace SufiChain.SufiPlatform.SufiCom.Channels;

/// <summary>
/// Well-known channel connector identifiers.
/// </summary>
public static class ChannelNames
{
    public const string Email = "Email";
    public const string Telegram = "Telegram";

    /// <summary>
    /// Telegram user-account connector (MTProto / TDLib). Distinct from <see cref="Telegram"/>,
    /// which is reserved for the future Bot API track.
    /// </summary>
    public const string TelegramUser = "TelegramUser";
    public const string SmsKavenegar = "Sms.Kavenegar";
    public const string SmsFanapMobile = "Sms.FanapMobile";
    public const string SmsIdehPardazan = "Sms.IdehPardazan";
    public const string VoiceKavenegar = "Voice.Kavenegar";
}
