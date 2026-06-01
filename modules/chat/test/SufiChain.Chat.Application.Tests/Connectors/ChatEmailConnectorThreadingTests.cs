using SufiChain.Chat.Connectors.Inbound;
using SufiChain.Chat.Connectors.Metadata;
using SufiChain.Chat.Messages;
using SufiChain.Chat.Sessions;
using Shouldly;
using Xunit;

namespace SufiChain.Chat.Connectors;

public class ChatEmailConnectorThreadingTests : ChatTestBase<ChatApplicationTestModule>
{
    private readonly IChatInboundMessageAppService _inboundMessageAppService;
    private readonly IChatSessionRepository _sessionRepository;
    private readonly IChatMessageRepository _messageRepository;

    public ChatEmailConnectorThreadingTests()
    {
        _inboundMessageAppService = GetRequiredService<IChatInboundMessageAppService>();
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

        var metadata = ChatSessionConnectorMetadataMapper.TryReadSessionMetadata(session.MetadataJson);
        metadata.ShouldNotBeNull();
        metadata!.ExternalThreadId.ShouldBe(threadId);
        metadata.LastExternalMessageId.ShouldBe("email-msg-002");
        metadata.ExternalParticipantAddress.ShouldBe("visitor@example.com");
        metadata.InReplyToExternalMessageId.ShouldBe("email-msg-001");

        var messages = await _messageRepository.GetListBySessionAsync(first.SessionId, includeInternal: false);
        messages.Count.ShouldBe(2);
    }

    private Task<IngestInboundChatMessageResult> IngestEmailAsync(
        string externalThreadId,
        string externalMessageId,
        string body,
        string? inReplyTo = null)
    {
        return _inboundMessageAppService.IngestAsync(new IngestInboundChatMessageInput
        {
            ConnectorName = "Test",
            ExternalThreadId = externalThreadId,
            ExternalMessageId = externalMessageId,
            InReplyToExternalMessageId = inReplyTo,
            Body = body,
            AccessMode = AccessMode.PublicAnonymous,
            ExternalParticipantAddress = "visitor@example.com",
            ExternalParticipantName = "Visitor Email",
            Sender = new ChatInboundSenderInput
            {
                AnonymousVisitorId = "visitor@example.com",
                SenderKind = ChatMessageSenderKind.Visitor,
                DisplayName = "Visitor Email"
            }
        });
    }
}
