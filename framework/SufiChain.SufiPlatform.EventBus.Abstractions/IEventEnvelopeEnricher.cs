namespace SufiChain.SufiPlatform.EventBus;

/// <summary>
/// Enriches integration-event envelopes at publication boundaries.
/// Implementations should populate missing identity, tenant, correlation,
/// causation, source, and trace metadata without overwriting explicit values.
/// </summary>
public interface IEventEnvelopeEnricher
{
    void Enrich(SufiIntegrationEto integrationEvent, string source, string sourceId);
}
