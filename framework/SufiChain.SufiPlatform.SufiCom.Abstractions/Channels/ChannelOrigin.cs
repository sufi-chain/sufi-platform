namespace SufiChain.SufiPlatform.SufiCom;

/// <summary>
/// Origin channel of a conversation or message.
/// </summary>
public enum ChannelOrigin
{
    Web,
    Email,
    Api,
    Widget,
    Admin,
    Sms,
    Voice,
    Telegram,
    /// <summary>
    /// Telegram user-account (MTProto / TDLib) conversations. Distinct from <see cref="Telegram"/>
    /// which is reserved for the Bot API track.
    /// </summary>
    TelegramUser
}
