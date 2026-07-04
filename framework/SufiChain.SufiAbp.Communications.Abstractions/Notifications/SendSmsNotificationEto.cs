using Volo.Abp.EventBus;

namespace SufiChain.SufiAbp.Communications.Notifications;

/// <summary>
/// Distributed event requesting an SMS delivery. Handled by the pro Messaging module
/// which renders the template (if any) and sends via the configured SMS provider.
/// </summary>
[Serializable]
[EventName("SufiAbp.Messaging.SendSmsNotification")]
public class SendSmsNotificationEto : NotificationMessageEto
{
    /// <summary>
    /// Recipient phone numbers (E.164).
    /// </summary>
    public List<string> To { get; set; } = new();

    /// <summary>
    /// Pre-localized message text. Used only when <see cref="TemplateName"/> is not set.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Optional SMS-channel template (compact, localized). When set, the handler
    /// renders <see cref="Message"/> from <see cref="TemplateData"/>.
    /// </summary>
    public string? TemplateName { get; set; }

    public Dictionary<string, object>? TemplateData { get; set; }
}
