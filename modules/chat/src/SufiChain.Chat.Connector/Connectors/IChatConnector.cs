using SufiChain.Chat.Connectors.Outbound;

namespace SufiChain.Chat.Connectors;

/// <summary>
/// Channel connector that can dispatch outbound messages from Chat to an external channel.
/// Inbound ingest is handled by <see cref="IChatInboundMessageAppService"/>.
/// </summary>
public interface IChatConnector
{
    /// <summary>
    /// Unique connector name (for example <see cref="ChatConnectorNames.Email"/>).
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Channel origin stamped on sessions and messages created through this connector.
    /// </summary>
    ChannelOrigin ChannelOrigin { get; }

    /// <summary>
    /// Default conversation kind for new sessions when the ingest input does not specify one.
    /// </summary>
    ConversationKind DefaultConversationKind { get; }

    /// <summary>
    /// Dispatches an outbound message to the external channel.
    /// </summary>
    Task<DispatchOutboundChatMessageResult> DispatchOutboundAsync(
        DispatchOutboundChatMessageInput input,
        CancellationToken cancellationToken = default);
}
