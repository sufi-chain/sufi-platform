namespace SufiChain.SufiPlatform.SufiCom.Notifications;

/// <summary>
/// Channel-agnostic notification request published via <see cref="INotificationPublisher"/>.
/// The default publisher fans this out into per-channel ETOs based on <see cref="Channels"/>
/// and the addresses available on <see cref="Recipients"/>.
/// </summary>
[Serializable]
public class NotificationEnvelope
{
    public Guid NotificationId { get; set; } = Guid.NewGuid();

    public Guid? TenantId { get; set; }

    /// <summary>
    /// Publishing module name, e.g. "HelpDesk.Ticketing".
    /// </summary>
    public string Source { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string? Culture { get; set; }

    public NotificationChannels Channels { get; set; } = NotificationChannels.None;

    public List<NotificationRecipient> Recipients { get; set; } = new();

    /// <summary>
    /// Shared template root (e.g. "Ticketing.TicketAssigned"). Each channel handler
    /// resolves its own suffix (e.g. ".Email", ".Sms"). When null, the fallback
    /// <see cref="InboxTitle"/>/<see cref="InboxBody"/> values are used as literal content.
    /// </summary>
    public string? TemplateName { get; set; }

    public Dictionary<string, object>? TemplateData { get; set; }

    /// <summary>
    /// Fallback title for the InApp channel and email subject when no template is used.
    /// </summary>
    public string? InboxTitle { get; set; }

    /// <summary>
    /// Fallback body for the InApp channel and email/SMS/voice content when no template is used.
    /// </summary>
    public string? InboxBody { get; set; }

    public NotificationSeverity Severity { get; set; } = NotificationSeverity.Info;

    public string? Url { get; set; }
}
