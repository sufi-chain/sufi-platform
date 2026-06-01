using SufiChain.Chat.Participants;
using SufiChain.Chat.Sessions;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Timing;
using Xunit;

namespace SufiChain.Chat.Sessions;

public class ChatSessionManager_Tests : ChatApplicationTestBase<ChatApplicationTestModule>
{
    private readonly ChatSessionManager _sessionManager;
    private readonly IChatSessionRepository _sessionRepository;
    private readonly IChatParticipantRepository _participantRepository;
    private readonly IClock _clock;

    public ChatSessionManager_Tests()
    {
        _sessionManager = GetRequiredService<ChatSessionManager>();
        _sessionRepository = GetRequiredService<IChatSessionRepository>();
        _participantRepository = GetRequiredService<IChatParticipantRepository>();
        _clock = GetRequiredService<IClock>();
    }

    [Fact]
    public async Task Should_Create_Open_Session()
    {
        var session = await _sessionManager.CreateAsync(
            "Support chat",
            AccessMode.PublicAnonymous,
            ConversationKind.Support,
            ChannelOrigin.Web);

        await _sessionRepository.InsertAsync(session, autoSave: true);

        session.Status.ShouldBe(ChatSessionStatus.Open);
        session.ConversationKind.ShouldBe(ConversationKind.Support);
        session.AccessMode.ShouldBe(AccessMode.PublicAnonymous);
    }

    [Fact]
    public async Task Should_Close_Session()
    {
        var session = await _sessionManager.CreateAsync(
            null,
            AccessMode.Internal,
            ConversationKind.Direct,
            ChannelOrigin.Web);

        await _sessionRepository.InsertAsync(session, autoSave: true);
        await _sessionManager.CloseAsync(session, ChatTestData.UserAId);

        session.Status.ShouldBe(ChatSessionStatus.Closed);
    }

    [Fact]
    public async Task Should_Return_Existing_Direct_Session_For_Same_User_Pair()
    {
        var first = await _sessionManager.GetOrCreateDirectSessionAsync(
            ChatTestData.UserAId,
            ChatTestData.UserBId);

        var second = await _sessionManager.GetOrCreateDirectSessionAsync(
            ChatTestData.UserAId,
            ChatTestData.UserBId);

        second.Id.ShouldBe(first.Id);

        var sessions = await _sessionRepository.GetListAsync();
        sessions.Count(session => session.ConversationKind == ConversationKind.Direct).ShouldBe(1);
    }

    [Fact]
    public async Task Should_Throw_When_Direct_Session_Has_Same_User_Twice()
    {
        await Should.ThrowAsync<BusinessException>(async () =>
        {
            await _sessionManager.GetOrCreateDirectSessionAsync(
                ChatTestData.UserAId,
                ChatTestData.UserAId);
        });
    }

    [Fact]
    public async Task Should_Enforce_Group_Participant_Limit()
    {
        var session = await _sessionManager.CreateAsync(
            "Large group",
            AccessMode.PublicAuthenticated,
            ConversationKind.Group,
            ChannelOrigin.Web);

        await _sessionRepository.InsertAsync(session, autoSave: true);

        for (var index = 0; index < 2; index++)
        {
            await _participantRepository.InsertAsync(new ChatParticipant(
                Guid.NewGuid(),
                null,
                session.Id,
                ChatMessageSenderKind.Visitor,
                _clock.Now,
                userId: Guid.NewGuid()), autoSave: true);
        }

        var exception = await Should.ThrowAsync<BusinessException>(async () =>
        {
            await _sessionManager.EnsureCanAddParticipantAsync(session, maxGroupParticipants: 2);
        });

        exception.Code.ShouldBe(ChatErrorCodes.GroupParticipantLimitExceeded);
    }
}
