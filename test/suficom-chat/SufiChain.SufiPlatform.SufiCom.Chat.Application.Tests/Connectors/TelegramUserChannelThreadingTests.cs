using SufiChain.SufiPlatform.SufiCom.Channels.Inbound;
using SufiChain.SufiPlatform.SufiCom.Chat;
using SufiChain.SufiPlatform.SufiCom.Chat.Messages;
using SufiChain.SufiPlatform.SufiCom.Chat.Sessions;
using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Channels;

/// <summary>
/// Session-axis tests for the Telegram user-account connector (ConnectionId-keyed sessions).
/// Mirrors <see cref="ChatEmailConnectorThreadingTests"/> but uses the
/// (connector, connectionId, externalThreadId) key to prove two phones sharing the same Telegram
/// thread id never collide, and follow-ups reuse the existing session.
/// </summary>
public class TelegramUserChannelThreadingTests : ChatApplicationTestBase<SufiComChatApplicationTestModule>
{
    private readonly IChannelInboundMessageAppService _inboundMessageAppService;
    private readonly IChatSessionRepository _sessionRepository;
    private readonly IChatMessageRepository _messageRepository;

    public TelegramUserChannelThreadingTests()
    {
        _inboundMessageAppService = GetRequiredService<IChannelInboundMessageAppService>();
        _sessionRepository = GetRequiredService<IChatSessionRepository>();
        _messageRepository = GetRequiredService<IChatMessageRepository>();
    }

    [Fact]
    public async Task Two_Phones_Should_Not_Collide_On_Same_Telegram_Thread()
    {
        const string threadId = "tg-chat-1";
        var connectionA = Guid.NewGuid();
        var connectionB = Guid.NewGuid();

        var firstA = await IngestTelegramAsync(connectionA, threadId, "tg-msg-a1", "Hello from phone A");
        var firstB = await IngestTelegramAsync(connectionB, threadId, "tg-msg-b1", "Hello from phone B");

        // Same thread id, different connections => two distinct sessions.
        firstA.CreatedNewSession.ShouldBeTrue();
        firstB.CreatedNewSession.ShouldBeTrue();
        firstA.SessionId.ShouldNotBe(firstB.SessionId);

        var sessionA = await _sessionRepository.GetAsync(firstA.SessionId);
        var sessionB = await _sessionRepository.GetAsync(firstB.SessionId);

        var metadataA = sessionA.GetConnectorMetadata();
        var metadataB = sessionB.GetConnectorMetadata();
        metadataA.ShouldNotBeNull();
        metadataB.ShouldNotBeNull();
        metadataA!.ExternalThreadId.ShouldBe(threadId);
        metadataB!.ExternalThreadId.ShouldBe(threadId);
        metadataA.ConnectionId.ShouldBe(connectionA);
        metadataB.ConnectionId.ShouldBe(connectionB);
        metadataA.LastExternalMessageId.ShouldBe("tg-msg-a1");
        metadataB.LastExternalMessageId.ShouldBe("tg-msg-b1");
    }

    [Fact]
    public async Task Follow_Up_On_Same_Connection_And_Thread_Should_Reuse_Session()
    {
        const string threadId = "tg-chat-1";
        var connectionA = Guid.NewGuid();

        var first = await IngestTelegramAsync(connectionA, threadId, "tg-msg-a1", "First message");
        first.CreatedNewSession.ShouldBeTrue();

        var followUp = await IngestTelegramAsync(
            connectionA,
            threadId,
            "tg-msg-a2",
            "Follow-up message",
            inReplyTo: "tg-msg-a1");

        followUp.CreatedNewSession.ShouldBeFalse();
        followUp.SessionId.ShouldBe(first.SessionId);

        var session = await _sessionRepository.GetAsync(first.SessionId);
        var metadata = session.GetConnectorMetadata();
        metadata.ShouldNotBeNull();
        metadata!.ConnectionId.ShouldBe(connectionA);
        metadata.LastExternalMessageId.ShouldBe("tg-msg-a2");
        metadata.InReplyToExternalMessageId.ShouldBe("tg-msg-a1");

        var messages = await _messageRepository.GetListBySessionAsync(first.SessionId, includeInternal: false);
        messages.Count.ShouldBe(2);
    }

    [Fact]
    public async Task Reply_On_Different_Connection_Should_Open_New_Session_Even_With_Same_Thread()
    {
        const string threadId = "tg-chat-1";
        var connectionA = Guid.NewGuid();
        var connectionB = Guid.NewGuid();

        var firstA = await IngestTelegramAsync(connectionA, threadId, "tg-msg-a1", "A");
        // Replying "to" the first message id but from a different connection must still isolate.
        var fromB = await IngestTelegramAsync(
            connectionB,
            threadId,
            "tg-msg-b1",
            "B",
            inReplyTo: "tg-msg-a1");

        fromB.SessionId.ShouldNotBe(firstA.SessionId);

        var sessionB = await _sessionRepository.GetAsync(fromB.SessionId);
        var metadataB = sessionB.GetConnectorMetadata();
        metadataB.ShouldNotBeNull();
        metadataB!.ConnectionId.ShouldBe(connectionB);
    }

    private Task<IngestInboundChannelMessageResult> IngestTelegramAsync(
        Guid connectionId,
        string externalThreadId,
        string externalMessageId,
        string body,
        string? inReplyTo = null)
    {
        return _inboundMessageAppService.IngestAsync(new IngestInboundChannelMessageInput
        {
            ConnectorName = ChannelNames.TelegramUser,
            ConnectionId = connectionId,
            ExternalThreadId = externalThreadId,
            ExternalMessageId = externalMessageId,
            InReplyToExternalMessageId = inReplyTo,
            Body = body,
            AccessMode = AccessMode.PublicAnonymous,
            ConversationKind = ConversationKind.Support,
            ExternalParticipantAddress = $"+98{connectionId.ToString("N").Substring(0, 10)}",
            ExternalParticipantName = "Telegram Visitor",
            Sender = new ChannelInboundSenderInput
            {
                AnonymousVisitorId = $"peer-{connectionId:N}-{externalThreadId}",
                SenderKind = ChatMessageSenderKind.Visitor,
                DisplayName = "Telegram Visitor"
            }
        });
    }
}
