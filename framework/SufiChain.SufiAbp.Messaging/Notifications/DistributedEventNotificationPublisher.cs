using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiAbp.Messaging.Notifications;

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
}
