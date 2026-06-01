using SufiChain.Chat.Connectors.Inbound;
using SufiChain.Chat.Connectors.Metadata;
using SufiChain.Chat.Messages;
using SufiChain.Chat.Sessions;
using Shouldly;
using Xunit;

namespace SufiChain.Chat.Connectors;

public class ChatInboundMessageAppServiceTests : ChatTestBase<ChatApplicationTestModule>
{
    private readonly IChatInboundMessageAppService _inboundMessageAppService;
    private readonly IChatSessionRepository _sessionRepository;
    private readonly IChatMessageRepository _messageRepository;

    public ChatInboundMessageAppServiceTests()
    {
        _inboundMessageAppService = GetRequiredService<IChatInboundMessageAppService>();
        _sessionRepository = GetRequiredService<IChatSessionRepository>();
        _messageRepository = GetRequiredService<IChatMessageRepository>();
    }

    [Fact]
    public async Task Should_continue_existing_session_for_same_external_thread()
    {
        const string externalThreadId = "thread-continue-001";

        var first = await _inboundMessageAppService.IngestAsync(new IngestInboundChatMessageInput
        {
            ConnectorName = "Test",
            ExternalThreadId = externalThreadId,
            ExternalMessageId = "msg-001",
            Body = "First inbound message",
            AccessMode = AccessMode.Internal,
            Sender = new ChatInboundSenderInput
            {
                AnonymousVisitorId = "visitor-001",
                SenderKind = ChatMessageSenderKind.Visitor,
                DisplayName = "Visitor"
            }
        });

        first.CreatedNewSession.ShouldBeTrue();

        var second = await _inboundMessageAppService.IngestAsync(new IngestInboundChatMessageInput
        {
            ConnectorName = "Test",
            ExternalThreadId = externalThreadId,
            ExternalMessageId = "msg-002",
            Body = "Second inbound message",
            AccessMode = AccessMode.Internal,
            Sender = new ChatInboundSenderInput
            {
                AnonymousVisitorId = "visitor-001",
                SenderKind = ChatMessageSenderKind.Visitor,
                DisplayName = "Visitor"
            }
        });

        second.CreatedNewSession.ShouldBeFalse();
        second.SessionId.ShouldBe(first.SessionId);

        var session = await _sessionRepository.GetAsync(first.SessionId);
        var metadata = ChatSessionConnectorMetadataMapper.TryReadSessionMetadata(session.MetadataJson);
        metadata.ShouldNotBeNull();
        metadata!.ExternalThreadId.ShouldBe(externalThreadId);
        metadata.LastExternalMessageId.ShouldBe("msg-002");

        var messages = await _messageRepository.GetListBySessionAsync(first.SessionId, includeInternal: false);
        messages.Count.ShouldBe(2);
    }
}
