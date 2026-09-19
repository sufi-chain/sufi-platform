using SufiChain.SufiPlatform.SufiCom.Chat.Messages;
using SufiChain.SufiPlatform.SufiCom.Chat.Participants;
using SufiChain.SufiPlatform.SufiCom.Chat.Sessions;
using SufiChain.SufiPlatform.Settings;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Users;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Chat.Sessions;

public class ChatSessionAppService_Tests : ChatApplicationTestBase<SufiComChatApplicationTestModule>
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
    public async Task Should_Filter_Inbox_Sessions_Excluding_Contextual_And_Internal()
    {
        using (CurrentUser.Change(ChatTestData.UserAId))
        {
            var inboxSession = await _sessionAppService.CreateAsync(new CreateChatSessionInput
            {
                Title = "Inbox AI",
                AccessMode = AccessMode.PublicAuthenticated,
                ConversationKind = ConversationKind.Assistant,
                ChannelOrigin = ChannelOrigin.Web,
                Origin = ChatSessionOrigin.Default,
                Participants =
                {
                    new AddChatParticipantInput
                    {
                        UserId = ChatTestData.UserAId,
                        ParticipantKind = ChatMessageSenderKind.Visitor
                    }
                }
            });

            var contextualSession = await _sessionAppService.CreateAsync(new CreateChatSessionInput
            {
                Title = "Contextual Hooshvare",
                AccessMode = AccessMode.Internal,
                ConversationKind = ConversationKind.Assistant,
                ChannelOrigin = ChannelOrigin.Admin,
                Origin = ChatSessionOrigin.Contextual,
                ExternalOrchestration = true,
                Participants =
                {
                    new AddChatParticipantInput
                    {
                        UserId = ChatTestData.UserAId,
                        ParticipantKind = ChatMessageSenderKind.Operator
                    }
                }
            });

            var legacyInternalSession = await _sessionAppService.CreateAsync(new CreateChatSessionInput
            {
                Title = "Legacy Internal Assistant",
                AccessMode = AccessMode.Internal,
                ConversationKind = ConversationKind.Assistant,
                ChannelOrigin = ChannelOrigin.Admin,
                Origin = ChatSessionOrigin.Default,
                Participants =
                {
                    new AddChatParticipantInput
                    {
                        UserId = ChatTestData.UserAId,
                        ParticipantKind = ChatMessageSenderKind.Operator
                    }
                }
            });

            // LiveChat / widget support: PublicAuthenticated + Contextual must stay out of portal inbox.
            var liveChatSupportSession = await _sessionAppService.CreateAsync(new CreateChatSessionInput
            {
                Title = "LiveChat Support",
                AccessMode = AccessMode.PublicAuthenticated,
                ConversationKind = ConversationKind.Support,
                ChannelOrigin = ChannelOrigin.Widget,
                Origin = ChatSessionOrigin.Contextual,
                ExternalOrchestration = true,
                Participants =
                {
                    new AddChatParticipantInput
                    {
                        UserId = ChatTestData.UserAId,
                        ParticipantKind = ChatMessageSenderKind.Visitor
                    }
                }
            });

            inboxSession.Origin.ShouldBe(ChatSessionOrigin.Default);
            contextualSession.Origin.ShouldBe(ChatSessionOrigin.Contextual);
            liveChatSupportSession.Origin.ShouldBe(ChatSessionOrigin.Contextual);

            var inboxFilter = await _sessionAppService.GetMySessionsAsync(new GetMyChatSessionsInput
            {
                Origin = ChatSessionOrigin.Default,
                AccessMode = AccessMode.PublicAuthenticated,
                MaxResultCount = 50
            });

            inboxFilter.Items.ShouldContain(session => session.Id == inboxSession.Id);
            inboxFilter.Items.ShouldNotContain(session => session.Id == contextualSession.Id);
            inboxFilter.Items.ShouldNotContain(session => session.Id == legacyInternalSession.Id);
            inboxFilter.Items.ShouldNotContain(session => session.Id == liveChatSupportSession.Id);

            var contextualFilter = await _sessionAppService.GetMySessionsAsync(new GetMyChatSessionsInput
            {
                Origin = ChatSessionOrigin.Contextual,
                MaxResultCount = 50
            });

            contextualFilter.Items.ShouldContain(session => session.Id == contextualSession.Id);
            contextualFilter.Items.ShouldNotContain(session => session.Id == inboxSession.Id);
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
        var exception = await Should.ThrowAsync<Volo.Abp.BusinessException>(async () =>
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

        var exception = await Should.ThrowAsync<Volo.Abp.BusinessException>(async () =>
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
