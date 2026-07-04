namespace SufiChain.SufiAbp.Communications;

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
    Telegram
}