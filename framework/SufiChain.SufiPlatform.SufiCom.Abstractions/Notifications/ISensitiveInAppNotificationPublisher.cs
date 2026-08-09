namespace SufiChain.SufiPlatform.SufiCom.Notifications;

/// <summary>
/// Publishes one sensitive notification directly to one host user's inbox.
/// Implementations must not fan out the request through channel delivery events
/// or copy its rendered content into generic delivery audit records.
/// </summary>
public interface ISensitiveInAppNotificationPublisher
{
    /// <summary>
    /// Renders and stores the notification once. Returns false when the stable
    /// notification id was already delivered to the recipient.
    /// </summary>
    Task<bool> PublishAsync(
        SensitiveInAppNotificationRequest request,
        CancellationToken cancellationToken = default);
}
