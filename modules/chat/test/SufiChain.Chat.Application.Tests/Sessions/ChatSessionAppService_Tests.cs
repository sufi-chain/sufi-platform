using SufiChain.Chat.Messages;
using SufiChain.Chat.Participants;
using SufiChain.Chat.Sessions;
using SufiChain.SufiAbp.SettingManagement;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Users;
using Xunit;

namespace SufiChain.Chat.Sessions;

public class ChatSessionAppService_Tests : ChatApplicationTestBase<ChatApplicationTestModule>
{
    private readonly IChatSessionAppService _sessionAppService;
    private readonly IChatSessionRepository _sessionRepository;

    public ChatSessionAppService_Tests()
    {
        _sessionAppService = GetRequiredService<IChatSessionAppService>();
        _sessionRepository = GetRequiredService<IChatSessionRepository>();
    }

    [Fact]
    public async Task Should_Create_Anonymous_Session()
    {
        var session = await _sessionAppService.CreateAsync(new CreateChatSessionInput
        {
            Title = "Anonymous support",
            AccessMode = AccessMode.PublicAnonymous,
            ConversationKind = ConversationKind.Support,
            ChannelOrigin = ChannelOrigin.Web,
            AnonymousVisitorId = ChatTestData.AnonymousVisitorId,
            Participants =
            {
                new AddChatParticipantInput
                {
                    AnonymousVisitorId = ChatTestData.AnonymousVisitorId,
                    ParticipantKind = ChatMessageSenderKind.Visitor,
                    DisplayName = "Visitor"
                }
            }
        });

        session.AccessMode.ShouldBe(AccessMode.PublicAnonymous);
        session.ConversationKind.ShouldBe(ConversationKind.Support);
        session.Participants.ShouldContain(participant =>
            participant.AnonymousVisitorId == ChatTestData.AnonymousVisitorId);
    }

    [Fact]
    public async Task Should_Create_Authenticated_Session()
    {
        using (CurrentUser.Change(ChatTestData.UserAId))
        {
            var session = await _sessionAppService.CreateAsync(new CreateChatSessionInput
            {
                Title = "Authenticated chat",
                AccessMode = AccessMode.PublicAuthenticated,
                ConversationKind = ConversationKind.Direct,
                Participants =
                {
                    new AddChatParticipantInput
                    {
                        UserId = ChatTestData.UserAId,
                        ParticipantKind = ChatMessageSenderKind.Visitor
                    },
                    new AddChatParticipantInput
                    {
                        UserId = ChatTestData.UserBId,
                        ParticipantKind = ChatMessageSenderKind.Visitor
                    }
                }
            });

            session.AccessMode.ShouldBe(AccessMode.PublicAuthenticated);
            session.Participants.Count.ShouldBe(2);
        }
    }

    [Fact]
    public async Task Should_Create_Or_Open_Assistant_Session()
    {
        using (CurrentUser.Change(ChatTestData.UserAId))
        {
            var created = await _sessionAppService.CreateAsync(new CreateChatSessionInput
            {
                Title = "AI Assistant",
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

            created.ConversationKind.ShouldBe(ConversationKind.Assistant);

            var mySessions = await _sessionAppService.GetMySessionsAsync(new GetMyChatSessionsInput
            {
                ConversationKind = ConversationKind.Assistant,
                MaxResultCount = 10
            });

            mySessions.Items.ShouldContain(session => session.Id == created.Id);
        }
    }

    [Fact]
    public async Task Should_Create_Direct_Session_Idempotently()
    {
        using (CurrentUser.Change(ChatTestData.UserAId))
        {
            var first = await _sessionAppService.GetOrCreateDirectSessionAsync(new GetOrCreateDirectSessionInput
            {
                OtherUserId = ChatTestData.UserBId
            });

            var second = await _sessionAppService.GetOrCreateDirectSessionAsync(new GetOrCreateDirectSessionInput
            {
                OtherUserId = ChatTestData.UserBId
            });

            second.Id.ShouldBe(first.Id);
        }
    }

    [Fact]
    public async Task Should_Create_Join_And_Leave_Group_Session()
    {
        Guid groupSessionId;

        using (CurrentUser.Change(ChatTestData.UserAId))
        {
            var group = await _sessionAppService.CreateGroupSessionAsync(new CreateGroupChatSessionInput
            {
                Title = "Project room",
                Participants =
                {
                    new AddChatParticipantInput
                    {
                        UserId = ChatTestData.UserBId,
                        ParticipantKind = ChatMessageSenderKind.Visitor
                    }
                }
            });

            groupSessionId = group.Id;
            group.ConversationKind.ShouldBe(ConversationKind.Group);
            group.Participants.Count.ShouldBeGreaterThanOrEqualTo(2);
        }

        using (CurrentUser.Change(ChatTestData.UserCId))
        {
            var joined = await _sessionAppService.JoinGroupSessionAsync(groupSessionId);

            joined.Participants.ShouldContain(participant => participant.UserId == ChatTestData.UserCId);

            await _sessionAppService.LeaveGroupSessionAsync(joined.Id);

            var refreshed = await _sessionAppService.GetAsync(joined.Id);
            refreshed.Participants
                .Where(participant => participant.UserId == ChatTestData.UserCId)
                .ShouldAllBe(participant => participant.LeftAt.HasValue);
        }
    }

    [Fact]
    public async Task Should_Deny_Anonymous_Session_When_Visitor_Identity_Missing()
    {
        var exception = await Should.ThrowAsync<BusinessException>(async () =>
        {
            await _sessionAppService.CreateAsync(new CreateChatSessionInput
            {
                AccessMode = AccessMode.PublicAnonymous,
                ConversationKind = ConversationKind.Support
            });
        });

        exception.Code.ShouldBe("AnonymousIdentityRequired");
    }

    [Fact]
    public async Task Should_Deny_When_Anonymous_Session_Cap_Exceeded()
    {
        var settingManager = GetRequiredService<ISettingManager>();
        await ChatTestSettingHelper.SetAnonymousUsagePolicyAsync(
            settingManager,
            maxSessionsPerUserPerDay: 1);

        await _sessionAppService.CreateAsync(new CreateChatSessionInput
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

        var exception = await Should.ThrowAsync<BusinessException>(async () =>
        {
            await _sessionAppService.CreateAsync(new CreateChatSessionInput
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
        });

        exception.Code.ShouldBe("AnonymousSessionLimitExceeded");
    }
}
