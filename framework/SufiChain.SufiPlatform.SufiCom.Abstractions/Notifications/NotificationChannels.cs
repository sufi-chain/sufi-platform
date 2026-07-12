namespace SufiChain.SufiPlatform.SufiCom.Notifications;

/// <summary>
/// Channel flags selected for a notification. Combinable (e.g. <c>InApp | Email</c>).
/// </summary>
[Flags]
public enum NotificationChannels
{
    None = 0,
    InApp = 1,
    Email = 2,
    Sms = 4,
    Voice = 8
}
