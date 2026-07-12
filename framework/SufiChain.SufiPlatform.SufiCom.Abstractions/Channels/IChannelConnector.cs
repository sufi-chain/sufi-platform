using SufiChain.SufiPlatform.SufiCom.Channels.Outbound;

namespace SufiChain.SufiPlatform.SufiCom.Channels;

/// <summary>
/// Bidirectional channel connector that can dispatch outbound messages to an external channel.
/// Inbound ingest is handled by <see cref="IChannelInboundMessageAppService"/>.
/// </summary>
public interface IChannelConnector : IChannel
{
    /// <summary>
    /// Default conversation kind for new sessions when the ingest input does not specify one.
    /// </summary>
    ConversationKind DefaultConversationKind { get; }

    /// <summary>
    /// Dispatches an outbound message to the external channel.
    /// </summary>
    Task<DispatchOutboundChannelMessageResult> DispatchOutboundAsync(
        DispatchOutboundChannelMessageInput input,
        CancellationToken cancellationToken = default);
}