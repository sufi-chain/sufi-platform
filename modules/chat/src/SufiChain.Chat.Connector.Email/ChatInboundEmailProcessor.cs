using SufiChain.Chat.Connectors.Metadata;
using SufiChain.Chat.Connectors.Email.Inbound;
using SufiChain.Chat.Connectors.Email.Settings;
using SufiChain.Chat.Connectors.Inbound;
using SufiChain.Chat.Sessions;
using Volo.Abp.DependencyInjection;

namespace SufiChain.Chat.Connectors.Email;

public class ChatInboundEmailProcessor : ITransientDependency
{
    protected IChatInboundEmailClient InboundEmailClient { get; }
    protected IChatInboundMessageAppService InboundMessageAppService { get; }
    protected IChatSessionRepository SessionRepository { get; }

    public ChatInboundEmailProcessor(
        IChatInboundEmailClient inboundEmailClient,
        IChatInboundMessageAppService inboundMessageAppService,
        IChatSessionRepository sessionRepository)
    {
        InboundEmailClient = inboundEmailClient;
        InboundMessageAppService = inboundMessageAppService;
        SessionRepository = sessionRepository;
    }

    public virtual async Task ProcessAsync(
        ChatEmailConnectorRuntimeSettings settings,
        Guid? tenantId,
        CancellationToken cancellationToken = default)
    {
        if (!settings.IsInboundConfigured)
        {
            return;
        }

        var messages = await InboundEmailClient.FetchUnreadAsync(settings, cancellationToken);
        foreach (var email in messages)
        {
            if (email.MessageId.IsNullOrWhiteSpace() || email.FromAddress.IsNullOrWhiteSpace())
            {
                continue;
            }

            var externalThreadId = await ResolveExternalThreadIdAsync(tenantId, email, cancellationToken);
            await InboundMessageAppService.IngestAsync(new IngestInboundChatMessageInput
            {
                ConnectorName = ChatConnectorNames.Email,
                ExternalThreadId = externalThreadId,
                ExternalMessageId = email.MessageId,
                InReplyToExternalMessageId = email.InReplyTo,
                Title = email.Subject,
                AccessMode = AccessMode.PublicAnonymous,
                ConversationKind = ConversationKind.Email,
                Body = email.Body,
                ExternalParticipantAddress = email.FromAddress,
                ExternalParticipantName = email.FromName,
                Sender = new ChatInboundSenderInput
                {
                    AnonymousVisitorId = email.FromAddress,
                    SenderKind = ChatMessageSenderKind.Visitor,
                    DisplayName = email.FromName ?? email.FromAddress
                }
            });
        }
    }

    protected virtual async Task<string> ResolveExternalThreadIdAsync(
        Guid? tenantId,
        ChatInboundEmailMessage email,
        CancellationToken cancellationToken)
    {
        var lookupIds = ChatEmailThreadResolver.BuildLookupIds(email.MessageId, email.InReplyTo, email.References);
        foreach (var lookupId in lookupIds)
        {
            var existingSession = await SessionRepository.FindByConnectorExternalThreadAsync(
                tenantId,
                ChatConnectorNames.Email,
                lookupId,
                cancellationToken);

            if (existingSession != null)
            {
                var metadata = ChatSessionConnectorMetadataMapper.TryReadSessionMetadata(existingSession.MetadataJson);
                return metadata?.ExternalThreadId ?? lookupId;
            }
        }

        return ChatEmailThreadResolver.ResolveExternalThreadId(email.MessageId, email.InReplyTo, email.References);
    }
}
