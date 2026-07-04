namespace SufiChain.SufiAbp.Communications.Notifications;

/// <summary>
/// Contributor to <see cref="INotificationRecipientResolver"/>. Product modules (e.g. HelpDesk
/// Ticketing) register an implementation that maps domain roles (Requester, Assignee,
/// OuManager, Agent) to <see cref="NotificationRecipient"/> entries. Address fields
/// (email/phone) may be left null for a contributor that only knows user ids; an
/// Identity-aware contributor or the host fills them in.
/// </summary>
public interface INotificationRecipientResolverContributor
{
    Task<List<NotificationRecipient>> ResolveAsync(
        NotificationRecipientRequest request,
        CancellationToken cancellationToken = default);
}
