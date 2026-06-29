namespace SufiChain.SufiAbp.Messaging.Notifications;

/// <summary>
/// Publishes in-app inbox notifications to be delivered to user inboxes.
/// Modules depend only on this abstraction; the inbox implementation
/// receives notifications via the distributed event bus.
/// </summary>
public interface INotificationPublisher
{
    /// <summary>
    /// Publishes a fully populated notification event.
    /// </summary>
    Task PublishAsync(InboxNotificationEto notification);

    /// <summary>
    /// Convenience overload to publish a notification to specific users.
    /// </summary>
    Task PublishAsync(
        string title,
        string? body,
        IEnumerable<Guid> userIds,
        InboxNotificationSeverity severity = InboxNotificationSeverity.Info,
        string? category = null,
        string? source = null,
        string? url = null,
        Dictionary<string, string>? data = null);

    /// <summary>
    /// Convenience overload to publish a notification to all users of the current tenant.
    /// </summary>
    Task PublishToAllAsync(
        string title,
        string? body,
        InboxNotificationSeverity severity = InboxNotificationSeverity.Info,
        string? category = null,
        string? source = null,
        string? url = null,
        Dictionary<string, string>? data = null);

    /// <summary>
    /// Publishes a multi-channel notification envelope. The default publisher fans this out
    /// into per-channel ETOs (<see cref="InboxNotificationEto"/>, <see cref="SendEmailNotificationEto"/>,
    /// <see cref="SendSmsNotificationEto"/>, <see cref="SendVoiceNotificationEto"/>) based on
    /// <see cref="NotificationEnvelope.Channels"/> and the addresses on
    /// <see cref="NotificationEnvelope.Recipients"/>. Channels lacking a resolvable address
    /// for a recipient are silently dropped (no throw).
    /// </summary>
    Task PublishAsync(NotificationEnvelope envelope, CancellationToken cancellationToken = default);
}
