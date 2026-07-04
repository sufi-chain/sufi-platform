using Volo.Abp.EventBus;

namespace SufiChain.SufiAbp.Communications.Notifications;

/// <summary>
/// Distributed event requesting an email delivery. Handled by the pro Messaging module
/// (or any host-registered handler) which renders the template and sends via <c>IEmailSender</c>.
/// </summary>
[Serializable]
[EventName("SufiAbp.Messaging.SendEmailNotification")]
public class SendEmailNotificationEto : NotificationMessageEto
{
    /// <summary>
    /// Resolved recipient email addresses.
    /// </summary>
    public List<string> To { get; set; } = new();

    public List<string>? Cc { get; set; }

    /// <summary>
    /// Pre-localized subject. Used only when <see cref="TemplateName"/> is not set.
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// HTML or plain body. Used only when <see cref="TemplateName"/> is not set.
    /// </summary>
    public string Body { get; set; } = string.Empty;

    public bool IsBodyHtml { get; set; } = true;

    /// <summary>
    /// When set, the handler renders subject + body via TextTemplating using
    /// <see cref="TemplateData"/> and <see cref="NotificationMessageEto.Culture"/>
    /// instead of using <see cref="Subject"/>/<see cref="Body"/> directly.
    /// </summary>
    public string? TemplateName { get; set; }

    public Dictionary<string, object>? TemplateData { get; set; }

    /// <summary>
    /// Optional FileManager file ids the handler resolves into attachments.
    /// </summary>
    public List<Guid>? AttachmentFileIds { get; set; }
}
