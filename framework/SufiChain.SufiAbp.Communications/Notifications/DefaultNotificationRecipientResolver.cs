using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiAbp.Communications.Notifications;

/// <summary>
/// Default <see cref="INotificationRecipientResolver"/>. Aggregates all registered
/// <see cref="INotificationRecipientResolverContributor"/>s and merges their results with
/// the explicit user ids from the request. Does not depend on Identity directly —
/// host/Identity wiring is supplied via contributors. De-duplicates by UserId when present.
/// </summary>
[Dependency(TryRegister = true)]
public class DefaultNotificationRecipientResolver : INotificationRecipientResolver, ITransientDependency
{
    protected IEnumerable<INotificationRecipientResolverContributor> Contributors { get; }

    public DefaultNotificationRecipientResolver(
        IEnumerable<INotificationRecipientResolverContributor> contributors)
    {
        Contributors = contributors;
    }

    public virtual async Task<List<NotificationRecipient>> ResolveAsync(
        NotificationRecipientRequest request,
        CancellationToken cancellationToken = default)
    {
        var results = new List<NotificationRecipient>();

        if (request.ExplicitUserIds is { Count: > 0 } explicitIds)
        {
            foreach (var userId in explicitIds)
            {
                results.Add(new NotificationRecipient { UserId = userId });
            }
        }

        foreach (var contributor in Contributors)
        {
            var contributed = await contributor.ResolveAsync(request, cancellationToken);
            if (contributed is null)
            {
                continue;
            }

            results.AddRange(contributed);
        }

        return Deduplicate(results);
    }

    protected virtual List<NotificationRecipient> Deduplicate(List<NotificationRecipient> recipients)
    {
        var byUserId = new Dictionary<Guid, NotificationRecipient>();
        var byEmail = new Dictionary<string, NotificationRecipient>(StringComparer.OrdinalIgnoreCase);
        var byPhone = new Dictionary<string, NotificationRecipient>(StringComparer.OrdinalIgnoreCase);
        var merged = new List<NotificationRecipient>();

        foreach (var recipient in recipients)
        {
            NotificationRecipient? target = null;

            if (recipient.UserId is { } userId && byUserId.TryGetValue(userId, out var byId))
            {
                target = byId;
            }
            else if (!string.IsNullOrWhiteSpace(recipient.Email) &&
                     byEmail.TryGetValue(recipient.Email!, out var byMail))
            {
                target = byMail;
            }
            else if (!string.IsNullOrWhiteSpace(recipient.PhoneNumber) &&
                     byPhone.TryGetValue(recipient.PhoneNumber!, out var byPhoneMatch))
            {
                target = byPhoneMatch;
            }

            if (target is null)
            {
                merged.Add(recipient);
                if (recipient.UserId is { } uid)
                {
                    byUserId[uid] = recipient;
                }
                if (!string.IsNullOrWhiteSpace(recipient.Email))
                {
                    byEmail[recipient.Email!] = recipient;
                }
                if (!string.IsNullOrWhiteSpace(recipient.PhoneNumber))
                {
                    byPhone[recipient.PhoneNumber!] = recipient;
                }
                continue;
            }

            target.UserId ??= recipient.UserId;
            target.Email ??= recipient.Email;
            target.PhoneNumber ??= recipient.PhoneNumber;
            target.DisplayName ??= recipient.DisplayName;
        }

        return merged;
    }
}
