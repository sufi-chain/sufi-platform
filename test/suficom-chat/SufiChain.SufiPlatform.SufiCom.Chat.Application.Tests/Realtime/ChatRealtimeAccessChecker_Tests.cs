using SufiChain.SufiPlatform.SufiCom;
using SufiChain.SufiPlatform.SufiCom.Chat.Participants;
using SufiChain.SufiPlatform.SufiCom.Chat.Realtime;
using SufiChain.SufiPlatform.SufiCom.Chat.Sessions;
using Shouldly;
using Volo.Abp.Users;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Chat.Realtime;

public class ChatRealtimeAccessChecker_Tests : ChatApplicationTestBase<SufiComChatApplicationTestModule>
{
    private readonly IChatRealtimeAccessChecker _accessChecker;
    private readonly IChatSessionAppService _sessionAppService;
    private readonly IChatSessionRepository _sessionRepository;
    private readonly IChatParticipantRepository _participantRepository;
    private readonly TestIdentityUserIntegrationService _users;

    public ChatRealtimeAccessChecker_Tests()
    {
        _accessChecker = GetRequiredService<IChatRealtimeAccessChecker>();
        _sessionAppService = GetRequiredService<IChatSessionAppService>();
        _sessionRepository = GetRequiredService<IChatSessionRepository>();
        _participantRepository = GetRequiredService<IChatParticipantRepository>();
        _users = GetRequiredService<TestIdentityUserIntegrationService>();
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
    public void Operators_Group_Name_Should_Be_Stable()
    {
        ChatRealtimeGroups.Operators().ShouldBe("livechat-operators");
    }

    [Fact]
    public async Task Tenant_user_without_a_tenant_claim_can_join_own_session()
    {
        SeedUser(ChatTestData.UserBId, "tenant-b-user", ChatTestData.TenantBId);
        var session = await InsertSessionAsync(ChatTestData.TenantBId, ChatTestData.UserBId);

        using (CurrentTenant.Change(ChatTestData.TenantBId))
        using (CurrentUser.Change(ChatTestData.UserBId))
        {
            var allowed = await _accessChecker.CanJoinSessionAsync(session.Id);
            allowed.ShouldBeTrue();
        }
    }

    [Fact]
    public async Task User_from_another_tenant_cannot_join_by_choosing_the_request_host()
    {
        SeedUser(ChatTestData.UserAId, "tenant-a-user", ChatTestData.TenantAId);
        var session = await InsertSessionAsync(ChatTestData.TenantBId, ChatTestData.UserAId);

        using (CurrentTenant.Change(ChatTestData.TenantBId))
        using (CurrentUser.Change(ChatTestData.UserAId))
        {
            var allowed = await _accessChecker.CanJoinSessionAsync(session.Id);
            allowed.ShouldBeFalse();
        }
    }

    [Fact]
    public async Task Host_user_cannot_join_a_tenant_session_by_choosing_the_request_host()
    {
        SeedUser(ChatTestData.UserAId, "host-user", tenantId: null);
        var session = await InsertSessionAsync(ChatTestData.TenantBId, ChatTestData.UserAId);

        using (CurrentTenant.Change(ChatTestData.TenantBId))
        using (CurrentUser.Change(ChatTestData.UserAId))
        {
            var allowed = await _accessChecker.CanJoinSessionAsync(session.Id);
            allowed.ShouldBeFalse();
        }
    }

    [Fact]
    public async Task Participant_from_another_tenant_cannot_join_by_choosing_the_request_host()
    {
        SeedUser(ChatTestData.UserAId, "tenant-a-user", ChatTestData.TenantAId);
        SeedUser(ChatTestData.UserBId, "tenant-b-user", ChatTestData.TenantBId);
        var session = await InsertSessionAsync(
            ChatTestData.TenantBId,
            ChatTestData.UserBId,
            participantUserId: ChatTestData.UserAId);

        using (CurrentTenant.Change(ChatTestData.TenantBId))
        using (CurrentUser.Change(ChatTestData.UserAId))
        {
            var allowed = await _accessChecker.CanJoinSessionAsync(session.Id);
            allowed.ShouldBeFalse();
        }
    }

    private void SeedUser(Guid userId, string userName, Guid? tenantId)
    {
        var user = _users.AddUser(userId, userName);
        user.TenantId = tenantId;
    }

    private async Task<ChatSession> InsertSessionAsync(Guid? tenantId, Guid creatorId, Guid? participantUserId = null)
    {
        using (CurrentTenant.Change(tenantId))
        {
            var session = new ChatSession(
                Guid.NewGuid(),
                tenantId,
                "join-check",
                AccessMode.PublicAuthenticated,
                ConversationKind.Support,
                ChannelOrigin.Web);
            session.AssignCreator(creatorId);
            await _sessionRepository.InsertAsync(session, autoSave: true);

            if (participantUserId.HasValue)
            {
                await _participantRepository.InsertAsync(
                    new ChatParticipant(
                        Guid.NewGuid(),
                        tenantId,
                        session.Id,
                        ChatMessageSenderKind.Operator,
                        DateTime.UtcNow,
                        userId: participantUserId),
                    autoSave: true);
            }

            return session;
        }
    }
}
