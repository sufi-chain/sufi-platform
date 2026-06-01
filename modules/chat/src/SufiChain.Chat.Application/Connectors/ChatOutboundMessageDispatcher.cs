using SufiChain.Chat.Connectors.Metadata;
using SufiChain.Chat.Connectors.Outbound;
using SufiChain.Chat.Messages;
using SufiChain.Chat.Sessions;
using Volo.Abp.DependencyInjection;

namespace SufiChain.Chat.Connectors;

public class ChatOutboundMessageDispatcher : ITransientDependency
{
    protected IChatConnectorRegistry ConnectorRegistry { get; }
    protected IChatSessionRepository SessionRepository { get; }
    protected IChatMessageRepository MessageRepository { get; }

    public ChatOutboundMessageDispatcher(
        IChatConnectorRegistry connectorRegistry,
        IChatSessionRepository sessionRepository,
        IChatMessageRepository messageRepository)
    {
        ConnectorRegistry = connectorRegistry;
        SessionRepository = sessionRepository;
        MessageRepository = messageRepository;
    }

    public virtual async Task TryDispatchAsync(ChatSession session, ChatMessage message, CancellationToken cancellationToken = default)
    {
        if (message.IsInternal || message.SenderKind != ChatMessageSenderKind.Operator)
        {
            return;
        }

        var connectorMetadata = ChatSessionConnectorMetadataMapper.TryReadSessionMetadata(session.MetadataJson);
        if (connectorMetadata == null)
        {
            return;
        }

        var connector = ConnectorRegistry.Find(connectorMetadata.ConnectorName);
        if (connector == null)
        {
            return;
        }

        var result = await connector.DispatchOutboundAsync(new DispatchOutboundChatMessageInput
        {
            SessionId = session.Id,
            MessageId = message.Id,
            Body = message.Body,
            ExternalThreadId = connectorMetadata.ExternalThreadId,
            ReplyToExternalMessageId = connectorMetadata.LastExternalMessageId,
            OperatorUserId = message.SenderUserId,
            MetadataJson = message.MetadataJson,
            SessionMetadataJson = session.MetadataJson
        }, cancellationToken);

        if (!result.Succeeded || result.ExternalMessageId.IsNullOrWhiteSpace())
        {
            return;
        }

        var previousExternalMessageId = connectorMetadata.LastExternalMessageId;

        message.SetMetadata(ChatSessionConnectorMetadataMapper.BuildMessageMetadata(new ChatMessageConnectorMetadata
        {
            ConnectorName = connector.Name,
            ExternalMessageId = result.ExternalMessageId!,
            InReplyToExternalMessageId = previousExternalMessageId
        }));

        connectorMetadata.LastExternalMessageId = result.ExternalMessageId;
        connectorMetadata.InReplyToExternalMessageId = previousExternalMessageId;
        session.SetMetadata(ChatSessionConnectorMetadataMapper.BuildSessionMetadata(connectorMetadata, session.MetadataJson));

        await MessageRepository.UpdateAsync(message, autoSave: true, cancellationToken: cancellationToken);
        await SessionRepository.UpdateAsync(session, autoSave: true, cancellationToken: cancellationToken);
    }
}
