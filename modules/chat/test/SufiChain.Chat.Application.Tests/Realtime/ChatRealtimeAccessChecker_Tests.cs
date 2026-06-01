using SufiChain.Chat.Participants;
using SufiChain.Chat.Realtime;
using SufiChain.Chat.Sessions;
using Shouldly;
using Volo.Abp.Users;
using Xunit;

namespace SufiChain.Chat.Realtime;

public class ChatRealtimeAccessChecker_Tests : ChatApplicationTestBase<ChatApplicationTestModule>
{
    private readonly IChatRealtimeAccessChecker _accessChecker;
    private readonly IChatSessionAppService _sessionAppService;

    public ChatRealtimeAccessChecker_Tests()
    {
        _accessChecker = GetRequiredService<IChatRealtimeAccessChecker>();
        _sessionAppService = GetRequiredService<IChatSessionAppService>();
    }

    [Fact]
    public async Task Participant_Should_Be_Allowed_To_Join_Session_Group()
    {
        var session = await _sessionAppService.CreateAsync(new CreateChatSessionInput
        {
            AccessMode = AccessMode.PublicAnonymous,
            ConversationKind = ConversationKind.Support,
            AnonymousVisitorId = ChatTestData.AnonymousVisitorId,
            Participants =
            {
                new AddChatParticipantInput
                {
                    AnonymousVisitorId = ChatTestData.AnonymousVisitorId,
                    ParticipantKind = ChatMessageSenderKind.Visitor
                }
            }
        });

        var allowed = await _accessChecker.CanJoinSessionAsync(session.Id, ChatTestData.AnonymousVisitorId);
        allowed.ShouldBeTrue();
    }

    [Fact]
    public void Session_Group_Name_Should_Be_Stable()
    {
        var sessionId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        ChatRealtimeGroups.Session(sessionId).ShouldBe("chat-session-44444444444444444444444444444444");
    }
}
