using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.EventBus;

/// <summary>
/// Base class for cross-module distributed event transfer objects.
/// Provides a stable idempotency key (<see cref="Id"/>), tenant scope, occurrence timestamp,
/// and distributed tracing metadata for Outbox/Inbox processing and cross-service correlation.
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

    /// <summary>
    /// Schema version of the integration event payload.
    /// Consumers should use this value to select a compatible contract handler.
    /// </summary>
    public int EventVersion { get; set; } = 1;

    /// <summary>
    /// Stable identifier for the end-to-end business operation that produced this event.
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Identifier of the command or event that directly caused this event.
    /// </summary>
    public string? CausationId { get; set; }

    /// <summary>
    /// Logical bounded-context or application source that emitted the event.
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// Identifier of the originating aggregate or document in <see cref="Source"/>.
    /// </summary>
    public string? SourceId { get; set; }

    /// <summary>
    /// W3C traceparent value captured when the event was produced, when available.
    /// </summary>
    public string? TraceParent { get; set; }

    /// <summary>
    /// Optional W3C tracestate value captured when the event was produced.
    /// </summary>
    public string? TraceState { get; set; }

    /// <summary>
    /// Additional transport-independent headers that must survive broker hops.
    /// Keep values small and non-sensitive.
    /// </summary>
    public Dictionary<string, string>? Headers { get; set; }
}
