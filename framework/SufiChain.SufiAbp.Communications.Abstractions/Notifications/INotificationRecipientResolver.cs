namespace SufiChain.SufiAbp.Communications.Notifications;

/// <summary>
/// Resolves <see cref="NotificationRecipient"/>s from a <see cref="NotificationRecipientRequest"/>.
/// Publishers call this to turn role-based / explicit user specs into concrete addresses
/// (email, phone, userId) without depending on Identity or the pro Messaging module.
/// </summary>
public interface INotificationRecipientResolver
{
    Task<List<NotificationRecipient>> ResolveAsync(
        NotificationRecipientRequest request,
        CancellationToken cancellationToken = default);
}
