namespace SufiChain.SufiAbp.Communications.Channels;

/// <summary>
/// Resolves registered <see cref="IChannelConnector"/> implementations by name.
/// </summary>
public interface IChannelConnectorRegistry
{
    IChannelConnector? Find(string connectorName);

    IReadOnlyList<IChannelConnector> GetAll();
}