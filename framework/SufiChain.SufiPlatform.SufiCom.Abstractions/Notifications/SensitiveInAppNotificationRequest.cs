namespace SufiChain.SufiPlatform.SufiCom.Notifications;

/// <summary>
/// Owner-only host inbox request for content that must not enter the distributed
/// notification fan-out or generic delivery audit pipeline.
/// </summary>
[Serializable]
public class SensitiveInAppNotificationRequest
{
    /// <summary>
    /// Stable idempotency identifier supplied by the calling workflow.
    /// </summary>
    public Guid NotificationId { get; set; }

    /// <summary>
    /// The single host user who may receive the notification.
    /// </summary>
    public Guid UserId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string TemplateName { get; set; } = string.Empty;

    public Dictionary<string, object> TemplateData { get; set; } = new();

    public string? Culture { get; set; }

    public string Source { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public NotificationSeverity Severity { get; set; } = NotificationSeverity.Info;

    public string? Url { get; set; }
}
