namespace SufiChain.Chat.Connectors;

/// <summary>
/// Resolves registered <see cref="IChatConnector"/> implementations by name.
/// </summary>
public interface IChatConnectorRegistry
{
    IChatConnector? Find(string connectorName);

    IReadOnlyList<IChatConnector> GetAll();
}
