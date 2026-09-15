using SufiChain.SufiPlatform.SufiCom.Chat.Messages;
using SufiChain.SufiPlatform.SufiCom.Chat.Participants;
using SufiChain.SufiPlatform.SufiCom.Chat.Sessions;
using SufiChain.SufiPlatform.SufiCom.Channels;
using SufiChain.SufiPlatform.SufiCom.Channels.Metadata;
using Shouldly;
using Volo.Abp.Uow;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Chat.MongoDB;

public class MongoChatWorkflow_Tests : ChatApplicationTestBase<SufiComChatMongoDbTestModule>
{
    private readonly IChatSessionAppService _sessionAppService;
    private readonly IChatMessageAppService _messageAppService;
    private readonly IChatSessionRepository _sessionRepository;

    public MongoChatWorkflow_Tests()
    {
        _sessionAppService = GetRequiredService<IChatSessionAppService>();
        _messageAppService = GetRequiredService<IChatMessageAppService>();
        _sessionRepository = GetRequiredService<IChatSessionRepository>();
    }

    [Fact]
    public async Task Should_Persist_Session_And_Messages_Through_MongoDB()
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
            Body = "Persisted via MongoDB",
            SenderKind = ChatMessageSenderKind.Visitor,
            AccessMode = AccessMode.Internal,
            AnonymousVisitorId = ChatTestData.AnonymousVisitorId
        });

        await WithUnitOfWorkAsync(async () =>
        {
            var reloaded = await _sessionRepository.GetAsync(session.Id);
            reloaded.LastMessageTime.ShouldNotBeNull();
            var messages = await GetRequiredService<IChatMessageRepository>()
                .GetListBySessionAsync(session.Id);
            messages.ShouldHaveSingleItem().Body.ShouldBe("Persisted via MongoDB");
        });
    }

    [Fact]
    public async Task Should_Scope_Direct_Session_Lookup_By_Tenant_In_MongoDB()
    {
        Guid tenantASessionId = Guid.Empty;
        Guid tenantBSessionId = Guid.Empty;
        using (CurrentTenant.Change(ChatTestData.TenantAId))
        {
            await WithUnitOfWorkAsync(async () =>
            {
                // The manager inserts the session and participants itself.
                var session = await GetRequiredService<ChatSessionManager>().GetOrCreateDirectSessionAsync(
                    ChatTestData.UserAId,
                    ChatTestData.UserBId);
                tenantASessionId = session.Id;
            });
        }

        using (CurrentTenant.Change(ChatTestData.TenantBId))
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var result = await _sessionRepository.FindDirectSessionByUserPairAsync(
                    ChatTestData.TenantBId,
                    ChatTestData.UserAId,
                    ChatTestData.UserBId);
                result.ShouldBeNull();
                (await _sessionRepository.FindAsync(tenantASessionId)).ShouldBeNull();

                var session = await GetRequiredService<ChatSessionManager>().GetOrCreateDirectSessionAsync(
                    ChatTestData.UserAId,
                    ChatTestData.UserBId);
                tenantBSessionId = session.Id;
                tenantBSessionId.ShouldNotBe(tenantASessionId);
            });
        }

        using (CurrentTenant.Change(ChatTestData.TenantAId))
        {
            await WithUnitOfWorkAsync(async () =>
            {
                // Reverse participant order must reuse this tenant's persisted conversation.
                var session = await GetRequiredService<ChatSessionManager>().GetOrCreateDirectSessionAsync(
                    ChatTestData.UserBId,
                    ChatTestData.UserAId);
                session.Id.ShouldBe(tenantASessionId);
                (await _sessionRepository.FindAsync(tenantBSessionId)).ShouldBeNull();
                (await GetRequiredService<IChatParticipantRepository>()
                    .GetActiveCountAsync(session.Id)).ShouldBe(2);
            });
        }
    }

    [Fact]
    public async Task Should_Resolve_Sessions_By_ConnectionId_And_Thread_In_MongoDB()
    {
        const string sharedThread = "tg-chat-shared-mongo";
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

    private async Task WithUnitOfWorkAsync(Func<Task> action)
    {
        using var unitOfWork = GetRequiredService<IUnitOfWorkManager>()
            .Begin(requiresNew: true, isTransactional: false);
        await action();
        await unitOfWork.CompleteAsync();
    }
}
