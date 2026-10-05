using SufiChain.SufiPlatform.SufiCom;
using SufiChain.SufiPlatform.SufiCom.Chat.Participants;
using SufiChain.SufiPlatform.SufiCom.Chat.Sessions;
using SufiChain.SufiPlatform.SufiCom.Chat.Settings;
using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Chat.Messages;

public class ChatAssistantReplyWatchdog_Tests : ChatApplicationTestBase<SufiComChatApplicationTestModule>
{
    private readonly IChatSessionAppService _sessionAppService;
    private readonly IChatMessageRepository _messageRepository;
    private readonly ChatAssistantReplyWatchdog _watchdog;

    public ChatAssistantReplyWatchdog_Tests()
    {
        _sessionAppService = GetRequiredService<IChatSessionAppService>();
        _messageRepository = GetRequiredService<IChatMessageRepository>();
        _watchdog = GetRequiredService<ChatAssistantReplyWatchdog>();
    }

    [Fact]
    public async Task Warns_when_an_assistant_session_has_no_reply()
    {
        var session = await CreateAssistantSessionAsync();
        var userMessage = await InsertAsync(session.Id, ChatMessageSenderKind.Visitor, "hello");

        var unanswered = await _watchdog.LogIfUnansweredAsync(
            session.TenantId,
            session.Id,
            userMessage.Id,
            userMessage.CreationTime,
            ChatAiResponseWait.DefaultSeconds);

        unanswered.ShouldBeTrue();
    }

    [Fact]
    public async Task Does_not_warn_after_an_assistant_reply()
    {
        var session = await CreateAssistantSessionAsync();
        var userMessage = await InsertAsync(session.Id, ChatMessageSenderKind.Visitor, "hello");
        await InsertAsync(session.Id, ChatMessageSenderKind.Assistant, "reply");

        var unanswered = await _watchdog.LogIfUnansweredAsync(
            session.TenantId,
            session.Id,
            userMessage.Id,
            userMessage.CreationTime,
            ChatAiResponseWait.DefaultSeconds);

        unanswered.ShouldBeFalse();
    }

    private async Task<ChatSessionDto> CreateAssistantSessionAsync()
    {
        using (CurrentUser.Change(ChatTestData.UserAId))
        {
            return await _sessionAppService.CreateAsync(new CreateChatSessionInput
            {
                Title = "Watchdog",
                AccessMode = AccessMode.PublicAuthenticated,
                ConversationKind = ConversationKind.Assistant,
                ChannelOrigin = ChannelOrigin.Web,
                Participants =
                {
                    new AddChatParticipantInput
                    {
                        UserId = ChatTestData.UserAId,
                        ParticipantKind = ChatMessageSenderKind.Visitor
                    }
                }
            });
        }
    }

    private async Task<ChatMessage> InsertAsync(Guid sessionId, ChatMessageSenderKind senderKind, string body)
    {
        var message = new ChatMessage(
            Guid.NewGuid(),
            tenantId: null,
            sessionId,
            body,
            senderKind);
        return await _messageRepository.InsertAsync(message, autoSave: true);
    }
}
