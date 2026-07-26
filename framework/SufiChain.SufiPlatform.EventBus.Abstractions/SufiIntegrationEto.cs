using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.EventBus;

/// <summary>
/// Base class for cross-module distributed event transfer objects.
/// Provides a stable idempotency key (<see cref="Id"/>), tenant scope, and occurrence timestamp
/// for Outbox/Inbox ordering and handler deduplication.
/// </summary>
[Serializable]
public abstract class SufiIntegrationEto : IMultiTenant
{
    /// <summary>
    /// Unique event id used as the Outbox/Inbox and handler dedupe key.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Tenant the event belongs to (null for host).
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// UTC timestamp when the domain change occurred (for ordering / replay).
    /// </summary>
    public DateTime OccurredAt { get; set; }
}
