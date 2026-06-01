using SufiChain.Chat.Messages;
using SufiChain.Chat.Participants;
using SufiChain.Chat.Sessions;
using Shouldly;
using Volo.Abp.Modularity;
using Xunit;

namespace SufiChain.Chat.EntityFrameworkCore;

[DependsOn(typeof(ChatApplicationTestModule))]
public class ChatEntityFrameworkCoreTestModule : AbpModule
{
}

public class EfCoreChatWorkflow_Tests : ChatApplicationTestBase<ChatEntityFrameworkCoreTestModule>
{
    private readonly IChatSessionRepository _sessionRepository;
    private readonly IChatSessionAppService _sessionAppService;
    private readonly IChatMessageAppService _messageAppService;

    public EfCoreChatWorkflow_Tests()
    {
        _sessionRepository = GetRequiredService<IChatSessionRepository>();
        _sessionAppService = GetRequiredService<IChatSessionAppService>();
        _messageAppService = GetRequiredService<IChatMessageAppService>();
    }

    [Fact]
    public async Task Should_Persist_Session_And_Messages_Through_EfCore()
    {
        var session = await _sessionAppService.CreateAsync(new CreateChatSessionInput
        {
            AccessMode = AccessMode.Internal,
            ConversationKind = ConversationKind.Support,
            Participants =
            {
                new AddChatParticipantInput
                {
                    AnonymousVisitorId = ChatTestData.AnonymousVisitorId,
                    ParticipantKind = ChatMessageSenderKind.Visitor
                }
            }
        });

        await _messageAppService.SendAsync(new SendChatMessageInput
        {
            SessionId = session.Id,
            Body = "Persisted via EF Core",
            SenderKind = ChatMessageSenderKind.Visitor,
            AccessMode = AccessMode.Internal,
            AnonymousVisitorId = ChatTestData.AnonymousVisitorId
        });

        var reloaded = await _sessionRepository.GetAsync(session.Id);
        reloaded.LastMessageTime.ShouldNotBeNull();
    }

    [Fact]
    public async Task Should_Scope_Direct_Session_Lookup_By_Tenant_In_EfCore()
    {
        using (CurrentTenant.Change(ChatTestData.TenantAId))
        {
            await _sessionRepository.InsertAsync(
                (await GetRequiredService<ChatSessionManager>().GetOrCreateDirectSessionAsync(
                    ChatTestData.UserAId,
                    ChatTestData.UserBId)),
                autoSave: true);
        }

        using (CurrentTenant.Change(ChatTestData.TenantBId))
        {
            var result = await _sessionRepository.FindDirectSessionByUserPairAsync(
                ChatTestData.TenantBId,
                ChatTestData.UserAId,
                ChatTestData.UserBId);

            result.ShouldBeNull();
        }
    }
}
