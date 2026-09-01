using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;

namespace SufiChain.SufiPlatform.EventBus;

/// <summary>
/// Default integration-event publication boundary for Sufi modules.
/// </summary>
public sealed class IntegrationEventPublisher : IIntegrationEventPublisher, ITransientDependency
{
    private readonly IDistributedEventBus _distributedEventBus;
    private readonly IEventEnvelopeEnricher _enricher;

    public IntegrationEventPublisher(
        IDistributedEventBus distributedEventBus,
        IEventEnvelopeEnricher enricher)
    {
        _distributedEventBus = distributedEventBus;
        _enricher = enricher;
    }

    public async Task PublishAsync<TEvent>(
        TEvent integrationEvent,
        string source,
        string sourceId,
        CancellationToken cancellationToken = default)
        where TEvent : SufiIntegrationEto
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);
        ArgumentException.ThrowIfNullOrWhiteSpace(source);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceId);

        cancellationToken.ThrowIfCancellationRequested();
        _enricher.Enrich(integrationEvent, source, sourceId);

        // ABP's distributed event bus enlists in the ambient Unit of Work and
        // persists the event through the configured Outbox when enabled.
        await _distributedEventBus.PublishAsync(integrationEvent);
    }
}
