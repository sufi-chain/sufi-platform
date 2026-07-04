using Volo.Abp.EventBus;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiAbp.Communications.Notifications;

/// <summary>
/// Common base for all channel delivery ETOs published by <see cref="INotificationPublisher"/>.
/// Carries correlation + tenant + localization context so outbox/retry works across services.
/// </summary>
[Serializable]
public abstract class NotificationMessageEto : IMultiTenant
{
    /// <summary>
    /// Correlates the fan-out of one logical notification across multiple channels.
    /// </summary>
    public Guid NotificationId { get; set; } = Guid.NewGuid();

    public Guid? TenantId { get; set; }

    /// <summary>
    /// Publishing module name, e.g. "HelpDesk.Ticketing".
    /// </summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// Logical grouping used in delivery logs / inbox tags.
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Optional localization hint used when rendering templates.
    /// </summary>
    public string? Culture { get; set; }

    public DateTime CreationTime { get; set; } = DateTime.UtcNow;
}
