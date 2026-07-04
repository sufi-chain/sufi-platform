namespace SufiChain.SufiAbp.Communications.Notifications;

/// <summary>
/// A role-based recipient specification used by <see cref="INotificationRecipientResolver"/>.
/// Product modules populate <see cref="Role"/> (e.g. "Requester", "Assignee", "OuManager")
/// and the context <see cref="EntityId"/>/<see cref="UserId"/>; a registered contributor
/// maps these to concrete users, and the default resolver fills email/phone from Identity.
/// </summary>
[Serializable]
public class NotificationRecipientRole
{
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// Context entity id (e.g. ticket id, OU id) the role is scoped to.
    /// </summary>
    public Guid? EntityId { get; set; }

    /// <summary>
    /// Already-resolved user id for the role, when known by the publisher.
    /// </summary>
    public Guid? UserId { get; set; }
}
