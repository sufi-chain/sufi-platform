using SufiChain.Chat.Connectors.Email.Settings;
using SufiChain.Chat.Connectors.Email.Templates;
using SufiChain.Chat.Connectors.Metadata;
using SufiChain.Chat.Connectors.Outbound;
using SufiChain.SufiAbp.TextTemplating;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Users;

namespace SufiChain.Chat.Connectors.Email;

public class ChatEmailConnector : IChatConnector, ISingletonDependency
{
    protected IChatEmailConnectorSettingsReader SettingsReader { get; }
    protected ChatEmailSmtpSender SmtpSender { get; }
    protected ITemplateRenderer TemplateRenderer { get; }
    protected ICurrentUser CurrentUser { get; }

    public ChatEmailConnector(
        IChatEmailConnectorSettingsReader settingsReader,
        ChatEmailSmtpSender smtpSender,
        ITemplateRenderer templateRenderer,
        ICurrentUser currentUser)
    {
        SettingsReader = settingsReader;
        SmtpSender = smtpSender;
        TemplateRenderer = templateRenderer;
        CurrentUser = currentUser;
    }

    public string Name => ChatConnectorNames.Email;

    public ChannelOrigin ChannelOrigin => ChannelOrigin.Email;

    public ConversationKind DefaultConversationKind => ConversationKind.Email;

    public virtual async Task<DispatchOutboundChatMessageResult> DispatchOutboundAsync(
        DispatchOutboundChatMessageInput input,
        CancellationToken cancellationToken = default)
    {
        var settings = await SettingsReader.GetAsync(cancellationToken);
        if (!settings.IsOutboundConfigured)
        {
            return new DispatchOutboundChatMessageResult
            {
                Succeeded = false,
                FailureReason = "Email connector outbound settings are not configured."
            };
        }

        var sessionMetadata = ChatSessionConnectorMetadataMapper.TryReadSessionMetadata(input.SessionMetadataJson);
        var recipient = sessionMetadata?.ExternalParticipantAddress;
        if (recipient.IsNullOrWhiteSpace())
        {
            return new DispatchOutboundChatMessageResult
            {
                Succeeded = false,
                FailureReason = "Email connector session is missing the external participant address."
            };
        }

        var body = await TemplateRenderer.RenderAsync(
            ChatEmailTemplateNames.Reply,
            new
            {
                operator_name = CurrentUser.Name ?? CurrentUser.UserName ?? "Support",
                message_body = input.Body,
                session_id = input.SessionId
            });

        var subject = BuildReplySubject(sessionMetadata?.ExternalThreadId, input.ExternalThreadId);
        var externalMessageId = await SmtpSender.SendAsync(
            settings,
            recipient,
            subject,
            body,
            settings.ReplyToAddress,
            input.ReplyToExternalMessageId,
            input.ExternalThreadId ?? sessionMetadata?.ExternalThreadId,
            cancellationToken);

        return new DispatchOutboundChatMessageResult
        {
            Succeeded = true,
            ExternalMessageId = externalMessageId
        };
    }

    protected virtual string BuildReplySubject(string? metadataThreadId, string? inputThreadId)
    {
        var threadId = inputThreadId ?? metadataThreadId;
        return threadId.IsNullOrWhiteSpace()
            ? "Re: Support request"
            : $"Re: Support request [{threadId}]";
    }
}
