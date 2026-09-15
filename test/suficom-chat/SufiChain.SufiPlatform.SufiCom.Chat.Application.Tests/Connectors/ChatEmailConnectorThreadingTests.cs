using SufiChain.SufiPlatform.SufiCom.Channels.Inbound;
using SufiChain.SufiPlatform.SufiCom.Chat;
using SufiChain.SufiPlatform.SufiCom.Chat.Messages;
using SufiChain.SufiPlatform.SufiCom.Chat.Sessions;
using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Channels;

public class ChatEmailConnectorThreadingTests : ChatTestBase<SufiComChatApplicationTestModule>
{
    private readonly IChannelInboundMessageAppService _inboundMessageAppService;
    private readonly IChatSessionRepository _sessionRepository;
    private readonly IChatMessageRepository _messageRepository;

    public ChatEmailConnectorThreadingTests()
    {
        _inboundMessageAppService = GetRequiredService<IChannelInboundMessageAppService>();
        _sessionRepository = GetRequiredService<IChatSessionRepository>();
        _messageRepository = GetRequiredService<IChatMessageRepository>();
    }

    [Fact]
    public async Task Should_continue_session_when_follow_up_uses_same_external_thread()
    {
        const string threadId = "email-thread-root@example.com";

        var first = await IngestEmailAsync(threadId, "email-msg-001", "First email");
        first.CreatedNewSession.ShouldBeTrue();

        var second = await IngestEmailAsync(threadId, "email-msg-002", "Follow-up email", inReplyTo: "email-msg-001");
        second.CreatedNewSession.ShouldBeFalse();
        second.SessionId.ShouldBe(first.SessionId);

        var session = await _sessionRepository.GetAsync(first.SessionId);
        session.ConversationKind.ShouldBe(ConversationKind.Support);

        var metadata = session.GetConnectorMetadata();
        metadata.ShouldNotBeNull();
        metadata!.ExternalThreadId.ShouldBe(threadId);
        metadata.LastExternalMessageId.ShouldBe("email-msg-002");
        metadata.ExternalParticipantAddress.ShouldBe("visitor@example.com");
        metadata.InReplyToExternalMessageId.ShouldBe("email-msg-001");

        var messages = await _messageRepository.GetListBySessionAsync(first.SessionId, includeInternal: false);
        messages.Count.ShouldBe(2);
    }

    private Task<IngestInboundChannelMessageResult> IngestEmailAsync(
        string externalThreadId,
        string externalMessageId,
        string body,
        string? inReplyTo = null)
    {
        return _inboundMessageAppService.IngestAsync(new IngestInboundChannelMessageInput
        {
            ConnectorName = "Test",
            ExternalThreadId = externalThreadId,
            ExternalMessageId = externalMessageId,
            InReplyToExternalMessageId = inReplyTo,
            Body = body,
            AccessMode = AccessMode.PublicAnonymous,
            ExternalParticipantAddress = "visitor@example.com",
            ExternalParticipantName = "Visitor Email",
            Sender = new ChannelInboundSenderInput
            {
                AnonymousVisitorId = "visitor@example.com",
                SenderKind = ChatMessageSenderKind.Visitor,
                DisplayName = "Visitor Email"
            }
        });
    }
}
