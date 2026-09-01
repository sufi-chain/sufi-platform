namespace SufiChain.SufiPlatform.EventBus;

/// <summary>
/// Publishes an explicit integration event after applying the platform event
/// envelope policy. The underlying distributed bus remains responsible for
/// Unit of Work participation and transactional Outbox persistence.
/// </summary>
public interface IIntegrationEventPublisher
{
    /// <summary>
    /// Enriches and publishes an integration event from the specified source.
    /// </summary>
    Task PublishAsync<TEvent>(
        TEvent integrationEvent,
        string source,
        string sourceId,
        CancellationToken cancellationToken = default)
        where TEvent : SufiIntegrationEto;
}
