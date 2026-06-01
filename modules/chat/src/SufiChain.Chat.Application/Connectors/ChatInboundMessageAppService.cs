using Microsoft.AspNetCore.Authorization;
using SufiChain.Chat.Connectors;
using SufiChain.Chat.Connectors.Inbound;
using SufiChain.Chat.Connectors.Metadata;
using SufiChain.Chat.ETOs;
using SufiChain.Chat.Mapping;
using SufiChain.Chat.Messages;
using SufiChain.Chat.Participants;
using SufiChain.Chat.Permissions;
using SufiChain.Chat.Realtime;
using SufiChain.Chat.Sessions;
using SufiChain.Chat.Usage;
using Volo.Abp;
using Volo.Abp.EventBus.Distributed;

namespace SufiChain.Chat.Connectors;

[Authorize(ChatPermissions.Inbox.Manage)]
public class ChatInboundMessageAppService : ChatAppService, IChatInboundMessageAppService
{
    protected IChatConnectorRegistry ConnectorRegistry { get; }
    protected IChatSessionRepository SessionRepository { get; }
    protected IChatParticipantRepository ParticipantRepository { get; }
    protected ChatSessionManager SessionManager { get; }
    protected ChatMessageManager MessageManager { get; }
    protected IChatUsageGuard UsageGuard { get; }
    protected ChatApplicationMapper Mapper { get; }
    protected IDistributedEventBus DistributedEventBus { get; }
    protected IChatRealtimeNotifier RealtimeNotifier { get; }

    public ChatInboundMessageAppService(
        IChatConnectorRegistry connectorRegistry,
        IChatSessionRepository sessionRepository,
        IChatParticipantRepository participantRepository,
        ChatSessionManager sessionManager,
        ChatMessageManager messageManager,
        IChatUsageGuard usageGuard,
        ChatApplicationMapper mapper,
        IDistributedEventBus distributedEventBus,
        IChatRealtimeNotifier realtimeNotifier)
    {
        ConnectorRegistry = connectorRegistry;
        SessionRepository = sessionRepository;
        ParticipantRepository = participantRepository;
        SessionManager = sessionManager;
        MessageManager = messageManager;
        UsageGuard = usageGuard;
        Mapper = mapper;
        DistributedEventBus = distributedEventBus;
        RealtimeNotifier = realtimeNotifier;
    }

    public virtual async Task<IngestInboundChatMessageResult> IngestAsync(IngestInboundChatMessageInput input)
    {
        var connector = ConnectorRegistry.Find(input.ConnectorName)
            ?? throw new BusinessException(ChatErrorCodes.ConnectorNotRegistered)
                .WithData("ConnectorName", input.ConnectorName);

        if (input.ExternalThreadId.IsNullOrWhiteSpace())
        {
            throw new BusinessException(ChatErrorCodes.ConnectorExternalThreadIdRequired);
        }

        var session = await SessionRepository.FindByConnectorExternalThreadAsync(
            CurrentTenant.Id,
            connector.Name,
            input.ExternalThreadId);

        var createdNewSession = session == null;
        if (session == null)
        {
            session = await CreateSessionFromInboundAsync(input, connector);
        }
        else
        {
            session = await UpdateSessionConnectorMetadataAsync(session, input, connector);
        }

        var messageMetadata = BuildMessageMetadata(input, connector);
        var usageResult = await UsageGuard.CheckCanSendMessageAsync(new ChatSendMessageContext
        {
            TenantId = CurrentTenant.Id,
            SessionId = session.Id,
            UserId = input.Sender.UserId,
            AnonymousVisitorId = input.Sender.AnonymousVisitorId,
            AnonymousClientIpHash = input.AnonymousClientIpHash,
            AccessMode = input.AccessMode,
            SenderKind = input.Sender.SenderKind
        });

        await EnsureUsageAllowedAsync(session.Id, usageResult);

        var message = await MessageManager.SendAsync(
            session,
            input.Body,
            input.Sender.SenderKind,
            input.Sender.UserId,
            input.Sender.AnonymousVisitorId,
            isInternal: false,
            isAuthorizedOperator: true,
            messageMetadata,
            input.AttachmentFileIds);

        await UsageGuard.RecordMessageSentAsync(message.SessionId, message.SenderKind);
        await PublishMessageSentAsync(message);

        var messageDto = Mapper.ToDto(message);
        await RealtimeNotifier.NotifyMessageSentAsync(messageDto);
        await RealtimeNotifier.NotifySessionUpdatedAsync(Mapper.ToDto(session));

        return new IngestInboundChatMessageResult
        {
            SessionId = session.Id,
            MessageId = message.Id,
            CreatedNewSession = createdNewSession
        };
    }

    protected virtual async Task<ChatSession> CreateSessionFromInboundAsync(
        IngestInboundChatMessageInput input,
        IChatConnector connector)
    {
        var usageResult = await UsageGuard.CheckCanStartSessionAsync(new ChatStartSessionContext
        {
            TenantId = CurrentTenant.Id,
            UserId = input.Sender.UserId,
            AnonymousVisitorId = input.Sender.AnonymousVisitorId,
            AnonymousClientIpHash = input.AnonymousClientIpHash,
            AccessMode = input.AccessMode,
            ConversationKind = input.ConversationKind ?? connector.DefaultConversationKind,
            ChannelOrigin = connector.ChannelOrigin
        });

        await EnsureUsageAllowedAsync(Guid.Empty, usageResult);

        var sessionMetadata = BuildSessionMetadata(input, connector);
        var session = await SessionManager.CreateAsync(
            input.Title,
            input.AccessMode,
            input.ConversationKind ?? connector.DefaultConversationKind,
            connector.ChannelOrigin,
            sessionMetadata);

        await SessionRepository.InsertAsync(session, autoSave: true);
        await EnsureInboundParticipantAsync(session, input.Sender);
        await PublishSessionCreatedAsync(session);

        return session;
    }

    protected virtual async Task<ChatSession> UpdateSessionConnectorMetadataAsync(
        ChatSession session,
        IngestInboundChatMessageInput input,
        IChatConnector connector)
    {
        session.SetMetadata(BuildSessionMetadata(input, connector, session.MetadataJson));
        await SessionRepository.UpdateAsync(session, autoSave: true);
        return session;
    }

    protected virtual async Task EnsureInboundParticipantAsync(ChatSession session, ChatInboundSenderInput sender)
    {
        if (sender.UserId.HasValue)
        {
            if (!await ParticipantRepository.IsParticipantAsync(session.Id, sender.UserId, null))
            {
                await ParticipantRepository.InsertAsync(new ChatParticipant(
                    GuidGenerator.Create(),
                    CurrentTenant.Id,
                    session.Id,
                    sender.SenderKind,
                    Clock.Now,
                    userId: sender.UserId,
                    displayName: sender.DisplayName), autoSave: true);
            }

            return;
        }

        if (!sender.AnonymousVisitorId.IsNullOrWhiteSpace()
            && !await ParticipantRepository.IsParticipantAsync(session.Id, null, sender.AnonymousVisitorId))
        {
            await ParticipantRepository.InsertAsync(new ChatParticipant(
                GuidGenerator.Create(),
                CurrentTenant.Id,
                session.Id,
                sender.SenderKind,
                Clock.Now,
                anonymousVisitorId: sender.AnonymousVisitorId,
                displayName: sender.DisplayName), autoSave: true);
        }
    }

    protected virtual string BuildSessionMetadata(
        IngestInboundChatMessageInput input,
        IChatConnector connector,
        string? existingMetadataJson = null)
    {
        var connectorMetadata = new ChatSessionConnectorMetadata
        {
            ConnectorName = connector.Name,
            ExternalThreadId = input.ExternalThreadId,
            LastExternalMessageId = input.ExternalMessageId,
            InReplyToExternalMessageId = input.InReplyToExternalMessageId,
            ExternalParticipantAddress = input.ExternalParticipantAddress,
            ExternalParticipantName = input.ExternalParticipantName ?? input.Sender.DisplayName
        };

        if (existingMetadataJson != null)
        {
            var existing = ChatSessionConnectorMetadataMapper.TryReadSessionMetadata(existingMetadataJson);
            if (connectorMetadata.ExternalParticipantAddress.IsNullOrWhiteSpace())
            {
                connectorMetadata.ExternalParticipantAddress = existing?.ExternalParticipantAddress;
            }

            if (connectorMetadata.ExternalParticipantName.IsNullOrWhiteSpace())
            {
                connectorMetadata.ExternalParticipantName = existing?.ExternalParticipantName;
            }
        }

        var mergeSource = existingMetadataJson ?? input.AdditionalMetadataJson;
        return ChatSessionConnectorMetadataMapper.BuildSessionMetadata(connectorMetadata, mergeSource);
    }

    protected virtual string? BuildMessageMetadata(IngestInboundChatMessageInput input, IChatConnector connector)
    {
        if (input.ExternalMessageId.IsNullOrWhiteSpace())
        {
            return null;
        }

        return ChatSessionConnectorMetadataMapper.BuildMessageMetadata(new ChatMessageConnectorMetadata
        {
            ConnectorName = connector.Name,
            ExternalMessageId = input.ExternalMessageId,
            InReplyToExternalMessageId = input.InReplyToExternalMessageId
        });
    }

    protected virtual async Task EnsureUsageAllowedAsync(Guid sessionId, ChatUsageCheckResult result)
    {
        if (!result.IsAllowed)
        {
            await RealtimeNotifier.NotifyUsageLimitExceededAsync(sessionId, Mapper.ToDto(result));
            throw new BusinessException(result.ReasonCode ?? ChatErrorCodes.UsageLimitExceeded);
        }
    }

    protected virtual async Task PublishSessionCreatedAsync(ChatSession session)
    {
        await DistributedEventBus.PublishAsync(new ChatSessionCreatedEto
        {
            Id = session.Id,
            TenantId = session.TenantId,
            Title = session.Title,
            AccessMode = session.AccessMode,
            ConversationKind = session.ConversationKind,
            ChannelOrigin = session.ChannelOrigin,
            CreatedAt = session.CreationTime
        });
    }

    protected virtual async Task PublishMessageSentAsync(ChatMessage message)
    {
        await DistributedEventBus.PublishAsync(new ChatMessageSentEto
        {
            Id = message.Id,
            TenantId = message.TenantId,
            SessionId = message.SessionId,
            SenderKind = message.SenderKind,
            SenderUserId = message.SenderUserId,
            AnonymousVisitorId = message.AnonymousVisitorId,
            IsInternal = message.IsInternal,
            SentAt = message.CreationTime
        });
    }
}
