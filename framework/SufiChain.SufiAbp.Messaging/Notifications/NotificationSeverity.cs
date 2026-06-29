namespace SufiChain.SufiAbp.Messaging.Notifications;

/// <summary>
/// Severity levels shared across all notification channels (in-app, email, SMS, voice).
/// </summary>
public enum NotificationSeverity
{
    Info = 1,
    Success = 2,
    Warning = 3,
    Error = 4
}
