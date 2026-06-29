namespace SufiChain.SufiAbp.Messaging.Notifications;

/// <summary>
/// Input to <see cref="INotificationRecipientResolver"/>.
/// </summary>
[Serializable]
public class NotificationRecipientRequest
{
    public Guid? TenantId { get; set; }

    /// <summary>
    /// Role-based recipients (e.g. requester, assignee, OU manager) resolved by contributors.
    /// </summary>
    public List<NotificationRecipientRole> Roles { get; set; } = new();

    /// <summary>
    /// Explicit user ids to include in addition to <see cref="Roles"/>.
    /// </summary>
    public List<Guid>? ExplicitUserIds { get; set; }
}
