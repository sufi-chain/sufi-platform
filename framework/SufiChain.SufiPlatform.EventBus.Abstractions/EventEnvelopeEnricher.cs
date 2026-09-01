using System.Diagnostics;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Timing;

namespace SufiChain.SufiPlatform.EventBus;

/// <summary>
/// Default publication-boundary enrichment for integration-event envelopes.
/// Explicit event values are preserved; missing values are filled from the
/// current tenant and W3C activity context.
/// </summary>
public sealed class EventEnvelopeEnricher : IEventEnvelopeEnricher, ITransientDependency
{
    private readonly ICurrentTenant _currentTenant;
    private readonly IClock _clock;

    public EventEnvelopeEnricher(ICurrentTenant currentTenant, IClock clock)
    {
        _currentTenant = currentTenant;
        _clock = clock;
    }

    public void Enrich(SufiIntegrationEto integrationEvent, string source, string sourceId)
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);

        if (integrationEvent.Id == Guid.Empty)
            integrationEvent.Id = Guid.NewGuid();
        if (integrationEvent.OccurredAt == default)
            integrationEvent.OccurredAt = _clock.Now;
        if (integrationEvent.TenantId is null)
            integrationEvent.TenantId = _currentTenant.Id;
        if (string.IsNullOrWhiteSpace(integrationEvent.Source))
            integrationEvent.Source = source;
        if (string.IsNullOrWhiteSpace(integrationEvent.SourceId))
            integrationEvent.SourceId = sourceId;

        var activity = Activity.Current;
        if (activity is null)
            return;

        integrationEvent.TraceParent ??= activity.Id;
        integrationEvent.TraceState ??= activity.TraceStateString;
        integrationEvent.CorrelationId ??= activity.TraceId.ToString();
        integrationEvent.CausationId ??= activity.ParentSpanId == default
            ? null
            : activity.ParentSpanId.ToString();
    }
}
