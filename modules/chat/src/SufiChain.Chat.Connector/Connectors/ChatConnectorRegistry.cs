using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace SufiChain.Chat.Connectors;

public class ChatConnectorRegistry : IChatConnectorRegistry, ISingletonDependency
{
    protected IReadOnlyDictionary<string, IChatConnector> Connectors { get; }

    public ChatConnectorRegistry(IEnumerable<IChatConnector> connectors)
    {
        Connectors = connectors
            .GroupBy(connector => connector.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);
    }

    public virtual IChatConnector? Find(string connectorName)
    {
        if (connectorName.IsNullOrWhiteSpace())
        {
            return null;
        }

        Connectors.TryGetValue(connectorName, out var connector);
        return connector;
    }

    public virtual IReadOnlyList<IChatConnector> GetAll()
    {
        return Connectors.Values.ToList();
    }
}
