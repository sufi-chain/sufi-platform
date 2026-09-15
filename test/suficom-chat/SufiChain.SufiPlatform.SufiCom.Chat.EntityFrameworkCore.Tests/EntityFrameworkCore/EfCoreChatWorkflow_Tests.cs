using SufiChain.SufiPlatform.SufiCom.Chat.Messages;
using SufiChain.SufiPlatform.SufiCom.Chat.Participants;
using SufiChain.SufiPlatform.SufiCom.Chat.Sessions;
using SufiChain.SufiPlatform.SufiCom.Channels;
using SufiChain.SufiPlatform.SufiCom.Channels.Metadata;
using Shouldly;
using Volo.Abp.Modularity;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Chat.EntityFrameworkCore;

[DependsOn(typeof(SufiComChatApplicationTestModule))]
public class SufiComChatEntityFrameworkCoreTestModule : AbpModule
{
}

public class EfCoreChatWorkflow_Tests : ChatApplicationTestBase<SufiComChatEntityFrameworkCoreTestModule>
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

    [Fact]
    public async Task Should_Resolve_Sessions_By_ConnectionId_And_Thread_In_EfCore()
    {
        const string sharedThread = "tg-chat-shared-ef";
        var supportConnectionId = Guid.NewGuid();
        var salesConnectionId = Guid.NewGuid();
        var sessionManager = GetRequiredService<ChatSessionManager>();

        var support = await _sessionRepository.InsertAsync(
            await sessionManager.CreateAsync(
                "Support",
                AccessMode.Internal,
                ConversationKind.Support,
                ChannelOrigin.Api,
                new ChannelSessionConnectorMetadata
                {
                    ConnectorName = ChannelNames.TelegramUser,
                    ExternalThreadId = sharedThread,
                    ConnectionId = supportConnectionId
                }),
            autoSave: true);

        var sales = await _sessionRepository.InsertAsync(
            await sessionManager.CreateAsync(
                "Sales",
                AccessMode.Internal,
                ConversationKind.Support,
                ChannelOrigin.Api,
                new ChannelSessionConnectorMetadata
                {
                    ConnectorName = ChannelNames.TelegramUser,
                    ExternalThreadId = sharedThread,
                    ConnectionId = salesConnectionId
                }),
            autoSave: true);

        var foundSupport = await _sessionRepository.FindByConnectorConnectionAndThreadAsync(
            null, ChannelNames.TelegramUser, supportConnectionId, sharedThread);
        var foundSales = await _sessionRepository.FindByConnectorConnectionAndThreadAsync(
            null, ChannelNames.TelegramUser, salesConnectionId, sharedThread);

        foundSupport.ShouldNotBeNull();
        foundSupport.Id.ShouldBe(support.Id);
        foundSales.ShouldNotBeNull();
        foundSales.Id.ShouldBe(sales.Id);
        foundSales.Id.ShouldNotBe(foundSupport.Id);
    }
}
