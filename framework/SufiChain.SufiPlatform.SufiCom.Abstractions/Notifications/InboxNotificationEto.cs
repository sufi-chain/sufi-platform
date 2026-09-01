using SufiChain.SufiPlatform.EventBus;
using Volo.Abp.EventBus;

namespace SufiChain.SufiPlatform.SufiCom.Notifications;

/// <summary>
/// Distributed event transfer object carrying an in-app inbox notification.
/// Published by any module via <see cref="INotificationPublisher"/> and consumed
/// by the inbox implementation (e.g. the Communication pro-module).
/// </summary>
[Serializable]
[EventName("Sufi.Communication.InboxNotification")]
public class InboxNotificationEto : SufiIntegrationEto
{
    /// <summary>
    /// Unique identifier of the published notification (correlation id for fan-out rows).
    /// </summary>
    public Guid NotificationId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Target user ids. Ignored when <see cref="ToAllUsers"/> is true.
    /// </summary>
    public List<Guid> UserIds { get; set; } = new();

    /// <summary>
    /// When true the notification targets all users of the tenant.
    /// </summary>
    public bool ToAllUsers { get; set; }

    /// <summary>
    /// Short notification title (required).
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Notification body text.
    /// </summary>
    public string? Body { get; set; }

    /// <summary>
    /// Severity of the notification.
    /// </summary>
    public InboxNotificationSeverity Severity { get; set; } = InboxNotificationSeverity.Info;

    /// <summary>
    /// Category name used to categorize the notification (mapped to a tag in the inbox).
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Optional URL to navigate to when the notification is clicked.
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// Optional extra payload data.
    /// </summary>
    public Dictionary<string, string> Data { get; set; } = new();

    /// <summary>
    /// UTC creation time of the notification (legacy; prefer <see cref="SufiIntegrationEto.OccurredAt"/>).
    /// </summary>
    public DateTime CreationTime { get; set; } = DateTime.UtcNow;
}
