using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiAbp.Communications.Notifications;

/// <summary>
/// Default <see cref="INotificationPublisher"/> implementation that publishes
/// <see cref="InboxNotificationEto"/> events over the distributed event bus.
/// </summary>
[Dependency(TryRegister = true)]
public class DistributedEventNotificationPublisher : INotificationPublisher, ITransientDependency
{
    protected IDistributedEventBus DistributedEventBus { get; }
    protected ICurrentTenant CurrentTenant { get; }

    public DistributedEventNotificationPublisher(
        IDistributedEventBus distributedEventBus,
        ICurrentTenant currentTenant)
    {
        DistributedEventBus = distributedEventBus;
        CurrentTenant = currentTenant;
    }

    public virtual async Task PublishAsync(InboxNotificationEto notification)
    {
        notification.TenantId ??= CurrentTenant.Id;
        await DistributedEventBus.PublishAsync(notification);
    }

    public virtual Task PublishAsync(
        string title,
        string? body,
        IEnumerable<Guid> userIds,
        InboxNotificationSeverity severity = InboxNotificationSeverity.Info,
        string? category = null,
        string? source = null,
        string? url = null,
        Dictionary<string, string>? data = null)
    {
        return PublishAsync(new InboxNotificationEto
        {
            Title = title,
            Body = body,
            UserIds = userIds.ToList(),
            Severity = severity,
            Category = category,
            Source = source,
            Url = url,
            Data = data ?? new Dictionary<string, string>()
        });
    }

    public virtual Task PublishToAllAsync(
        string title,
        string? body,
        InboxNotificationSeverity severity = InboxNotificationSeverity.Info,
        string? category = null,
        string? source = null,
        string? url = null,
        Dictionary<string, string>? data = null)
    {
        return PublishAsync(new InboxNotificationEto
        {
            Title = title,
            Body = body,
            ToAllUsers = true,
            Severity = severity,
            Category = category,
            Source = source,
            Url = url,
            Data = data ?? new Dictionary<string, string>()
        });
    }

    public virtual async Task PublishAsync(
        NotificationEnvelope envelope,
        CancellationToken cancellationToken = default)
    {
        envelope.TenantId ??= CurrentTenant.Id;

        if (envelope.Channels.HasFlag(NotificationChannels.InApp))
        {
            await PublishInAppAsync(envelope, cancellationToken);
        }

        if (envelope.Channels.HasFlag(NotificationChannels.Email))
        {
            await PublishEmailAsync(envelope, cancellationToken);
        }

        if (envelope.Channels.HasFlag(NotificationChannels.Sms))
        {
            await PublishSmsAsync(envelope, cancellationToken);
        }

        if (envelope.Channels.HasFlag(NotificationChannels.Voice))
        {
            await PublishVoiceAsync(envelope, cancellationToken);
        }
    }

    protected virtual async Task PublishInAppAsync(
        NotificationEnvelope envelope,
        CancellationToken cancellationToken)
    {
        var userIds = envelope.Recipients
            .Where(r => r.UserId.HasValue)
            .Select(r => r.UserId!.Value)
            .ToList();

        if (userIds.Count == 0)
        {
            return;
        }

        await DistributedEventBus.PublishAsync(new InboxNotificationEto
        {
            NotificationId = envelope.NotificationId,
            TenantId = envelope.TenantId,
            UserIds = userIds,
            Title = envelope.InboxTitle ?? string.Empty,
            Body = envelope.InboxBody,
            Severity = (InboxNotificationSeverity)envelope.Severity,
            Category = envelope.Category,
            Source = envelope.Source,
            Url = envelope.Url

        });
    }

    protected virtual async Task PublishEmailAsync(
        NotificationEnvelope envelope,
        CancellationToken cancellationToken)
    {
        var to = AddressesFor(envelope.Recipients, r => r.Email);
        if (to.Count == 0)
        {
            return;
        }

        await DistributedEventBus.PublishAsync(new SendEmailNotificationEto
        {
            NotificationId = envelope.NotificationId,
            TenantId = envelope.TenantId,
            Source = envelope.Source,
            Category = envelope.Category,
            Culture = envelope.Culture,
            To = to,
            Subject = envelope.InboxTitle ?? string.Empty,
            Body = envelope.InboxBody ?? string.Empty,
            TemplateName = envelope.TemplateName,
            TemplateData = envelope.TemplateData
        });
    }

    protected virtual async Task PublishSmsAsync(
        NotificationEnvelope envelope,
        CancellationToken cancellationToken)
    {
        var to = AddressesFor(envelope.Recipients, r => r.PhoneNumber);
        if (to.Count == 0)
        {
            return;
        }

        await DistributedEventBus.PublishAsync(new SendSmsNotificationEto
        {
            NotificationId = envelope.NotificationId,
            TenantId = envelope.TenantId,
            Source = envelope.Source,
            Category = envelope.Category,
            Culture = envelope.Culture,
            To = to,
            Message = envelope.InboxBody ?? string.Empty,
            TemplateName = envelope.TemplateName,
            TemplateData = envelope.TemplateData
        });
    }

    protected virtual async Task PublishVoiceAsync(
        NotificationEnvelope envelope,
        CancellationToken cancellationToken)
    {
        var to = AddressesFor(envelope.Recipients, r => r.PhoneNumber);
        if (to.Count == 0)
        {
            return;
        }

        await DistributedEventBus.PublishAsync(new SendVoiceNotificationEto
        {
            NotificationId = envelope.NotificationId,
            TenantId = envelope.TenantId,
            Source = envelope.Source,
            Category = envelope.Category,
            Culture = envelope.Culture,
            To = to,
            Text = envelope.InboxBody ?? string.Empty,
            TemplateName = envelope.TemplateName,
            TemplateData = envelope.TemplateData
        });
    }

    protected virtual List<string> AddressesFor(
        IEnumerable<NotificationRecipient> recipients,
        Func<NotificationRecipient, string?> selector)
    {
        return recipients
            .Select(selector)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
