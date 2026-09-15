using SufiChain.SufiPlatform.SufiCom.Channels.Inbound;
using SufiChain.SufiPlatform.SufiCom.Chat;
using SufiChain.SufiPlatform.SufiCom.Chat.Messages;
using SufiChain.SufiPlatform.SufiCom.Chat.Sessions;
using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Channels;

public class ChatInboundMessageAppServiceTests : ChatTestBase<SufiComChatApplicationTestModule>
{
    private readonly IChannelInboundMessageAppService _inboundMessageAppService;
    private readonly IChatSessionRepository _sessionRepository;
    private readonly IChatMessageRepository _messageRepository;

    public ChatInboundMessageAppServiceTests()
    {
        _inboundMessageAppService = GetRequiredService<IChannelInboundMessageAppService>();
        _sessionRepository = GetRequiredService<IChatSessionRepository>();
        _messageRepository = GetRequiredService<IChatMessageRepository>();
    }

    [Fact]
    public async Task Should_continue_existing_session_for_same_external_thread()
    {
        const string externalThreadId = "thread-continue-001";

        var first = await _inboundMessageAppService.IngestAsync(new IngestInboundChannelMessageInput
        {
            ConnectorName = "Test",
            ExternalThreadId = externalThreadId,
            ExternalMessageId = "msg-001",
            Body = "First inbound message",
            AccessMode = AccessMode.Internal,
            Sender = new ChannelInboundSenderInput
            {
                AnonymousVisitorId = "visitor-001",
                SenderKind = ChatMessageSenderKind.Visitor,
                DisplayName = "Visitor"
            }
        });

        first.CreatedNewSession.ShouldBeTrue();

        var second = await _inboundMessageAppService.IngestAsync(new IngestInboundChannelMessageInput
        {
            ConnectorName = "Test",
            ExternalThreadId = externalThreadId,
            ExternalMessageId = "msg-002",
            Body = "Second inbound message",
            AccessMode = AccessMode.Internal,
            Sender = new ChannelInboundSenderInput
            {
                AnonymousVisitorId = "visitor-001",
                SenderKind = ChatMessageSenderKind.Visitor,
                DisplayName = "Visitor"
            }
        });

        second.CreatedNewSession.ShouldBeFalse();
        second.SessionId.ShouldBe(first.SessionId);

        var session = await _sessionRepository.GetAsync(first.SessionId);
        var metadata = session.GetConnectorMetadata();
        metadata.ShouldNotBeNull();
        metadata!.ExternalThreadId.ShouldBe(externalThreadId);
        metadata.LastExternalMessageId.ShouldBe("msg-002");

        var messages = await _messageRepository.GetListBySessionAsync(first.SessionId, includeInternal: false);
        messages.Count.ShouldBe(2);
    }

    [Fact]
    public async Task Should_Create_Distinct_Sessions_For_Different_ConnectionIds_On_Same_Thread()
    {
        const string externalThreadId = "tg-chat-shared-001";
        var supportConnectionId = Guid.NewGuid();
        var salesConnectionId = Guid.NewGuid();

        var support = await IngestWithConnectionAsync(externalThreadId, supportConnectionId, "msg-support-001");
        var sales = await IngestWithConnectionAsync(externalThreadId, salesConnectionId, "msg-sales-001");

        support.CreatedNewSession.ShouldBeTrue();
        sales.CreatedNewSession.ShouldBeTrue();
        sales.SessionId.ShouldNotBe(support.SessionId);

        var supportSession = await _sessionRepository.GetAsync(support.SessionId);
        var supportMetadata = supportSession.GetConnectorMetadata();
        supportMetadata.ShouldNotBeNull();
        supportMetadata!.ConnectionId.ShouldBe(supportConnectionId);

        var salesSession = await _sessionRepository.GetAsync(sales.SessionId);
        var salesMetadata = salesSession.GetConnectorMetadata();
        salesMetadata.ShouldNotBeNull();
        salesMetadata!.ConnectionId.ShouldBe(salesConnectionId);
    }

    [Fact]
    public async Task Should_Continue_Same_Session_For_Same_ConnectionId_And_Thread()
    {
        const string externalThreadId = "tg-chat-continue-001";
        var connectionId = Guid.NewGuid();

        var first = await IngestWithConnectionAsync(externalThreadId, connectionId, "msg-001");
        var second = await IngestWithConnectionAsync(externalThreadId, connectionId, "msg-002");

        first.CreatedNewSession.ShouldBeTrue();
        second.CreatedNewSession.ShouldBeFalse();
        second.SessionId.ShouldBe(first.SessionId);
    }

    [Fact]
    public async Task Should_Return_Existing_Message_When_External_Message_Is_Retried()
    {
        const string externalThreadId = "email-retry-thread-001";
        const string externalMessageId = "email-retry-message-001";

        var first = await IngestWithConnectionAsync(
            externalThreadId,
            Guid.Empty,
            externalMessageId);
        var second = await IngestWithConnectionAsync(
            externalThreadId,
            Guid.Empty,
            "email-retry-message-002");
        var retry = await IngestWithConnectionAsync(
            externalThreadId,
            Guid.Empty,
            externalMessageId);

        second.MessageId.ShouldNotBe(first.MessageId);
        retry.CreatedNewSession.ShouldBeFalse();
        retry.SessionId.ShouldBe(first.SessionId);
        retry.MessageId.ShouldBe(first.MessageId);

        var messages = await _messageRepository.GetListBySessionAsync(
            first.SessionId,
            includeInternal: true);
        messages.Count.ShouldBe(2);

        var session = await _sessionRepository.GetAsync(first.SessionId);
        session.GetConnectorMetadata()!.LastExternalMessageId
            .ShouldBe("email-retry-message-002");
    }

    private async Task<IngestInboundChannelMessageResult> IngestWithConnectionAsync(
        string externalThreadId,
        Guid connectionId,
        string externalMessageId)
    {
        return await _inboundMessageAppService.IngestAsync(new IngestInboundChannelMessageInput
        {
            ConnectorName = "Test",
            ExternalThreadId = externalThreadId,
            ExternalMessageId = externalMessageId,
            Body = "Inbound message",
            AccessMode = AccessMode.Internal,
            ConnectionId = connectionId,
            Sender = new ChannelInboundSenderInput
            {
                AnonymousVisitorId = "visitor-" + externalMessageId,
                SenderKind = ChatMessageSenderKind.Visitor,
                DisplayName = "Visitor"
            }
        });
    }
}
