using SufiChain.Chat.Contacts;
using SufiChain.Chat.ConversationLinks;
using SufiChain.Chat.Sessions;
using SufiChain.Chat.Supports;
using Shouldly;
using Volo.Abp;
using Volo.Abp.MultiTenancy;
using Xunit;

namespace SufiChain.Chat;

public class ChatTenantIsolation_Tests : ChatApplicationTestBase<ChatApplicationTestModule>
{
    private readonly IChatSessionAppService _sessionAppService;
    private readonly IChatSessionRepository _sessionRepository;
    private readonly IConversationLinkAppService _conversationLinkAppService;
    private readonly IChatContactAppService _contactAppService;
    private readonly TestChatContactProvider _contactProvider;

    public ChatTenantIsolation_Tests()
    {
        _sessionAppService = GetRequiredService<IChatSessionAppService>();
        _sessionRepository = GetRequiredService<IChatSessionRepository>();
        _conversationLinkAppService = GetRequiredService<IConversationLinkAppService>();
        _contactAppService = GetRequiredService<IChatContactAppService>();
        _contactProvider = GetRequiredService<TestChatContactProvider>();
    }

    [Fact]
    public async Task Tenant_A_Should_Not_See_Tenant_B_Session()
    {
        Guid tenantASessionId;

        using (CurrentTenant.Change(ChatTestData.TenantAId))
        {
            var session = await _sessionAppService.CreateAsync(new CreateChatSessionInput
            {
                AccessMode = AccessMode.Internal,
                ConversationKind = ConversationKind.Support,
                Participants =
                {
                    new AddChatParticipantInput
                    {
                        UserId = ChatTestData.UserAId,
                        ParticipantKind = ChatMessageSenderKind.Visitor
                    }
                }
            });

            tenantASessionId = session.Id;
        }

        using (CurrentTenant.Change(ChatTestData.TenantBId))
        {
            await Should.ThrowAsync<Exception>(async () =>
            {
                await _sessionAppService.GetAsync(tenantASessionId);
            });
        }
    }

    [Fact]
    public async Task Direct_Session_Lookup_Should_Be_Tenant_Scoped()
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
            var tenantBDirect = await _sessionRepository.FindDirectSessionByUserPairAsync(
                ChatTestData.TenantBId,
                ChatTestData.UserAId,
                ChatTestData.UserBId);

            tenantBDirect.ShouldBeNull();
        }

        using (CurrentTenant.Change(ChatTestData.TenantAId))
        {
            var tenantADirect = await _sessionRepository.FindDirectSessionByUserPairAsync(
                ChatTestData.TenantAId,
                ChatTestData.UserAId,
                ChatTestData.UserBId);

            tenantADirect.ShouldNotBeNull();
        }
    }

    [Fact]
    public async Task Contact_Search_Should_Be_Tenant_Scoped()
    {
        _contactProvider.SeedContact(ChatTestData.TenantAId, new ChatContactDto
        {
            UserId = ChatTestData.UserAId,
            DisplayName = "Tenant A Contact"
        });

        _contactProvider.SeedContact(ChatTestData.TenantBId, new ChatContactDto
        {
            UserId = ChatTestData.UserBId,
            DisplayName = "Tenant B Contact"
        });

        using (CurrentTenant.Change(ChatTestData.TenantAId))
        {
            var tenantAContacts = await _contactAppService.SearchAsync(new SearchChatContactsInput
            {
                Filter = "Tenant",
                MaxResultCount = 10
            });

            tenantAContacts.Items.ShouldAllBe(contact => contact.DisplayName.Contains("Tenant A"));
        }

        using (CurrentTenant.Change(ChatTestData.TenantBId))
        {
            var tenantBContacts = await _contactAppService.SearchAsync(new SearchChatContactsInput
            {
                Filter = "Tenant",
                MaxResultCount = 10
            });

            tenantBContacts.Items.ShouldAllBe(contact => contact.DisplayName.Contains("Tenant B"));
        }
    }

    [Fact]
    public async Task Conversation_Links_Should_Be_Tenant_Scoped()
    {
        Guid tenantALinkId;
        Guid tenantASessionId;

        using (CurrentTenant.Change(ChatTestData.TenantAId))
        {
            var session = await _sessionAppService.CreateAsync(new CreateChatSessionInput
            {
                AccessMode = AccessMode.Internal,
                ConversationKind = ConversationKind.Support,
                Participants =
                {
                    new AddChatParticipantInput
                    {
                        UserId = ChatTestData.UserAId,
                        ParticipantKind = ChatMessageSenderKind.Visitor
                    }
                }
            });

            tenantASessionId = session.Id;

            var link = await _conversationLinkAppService.CreateAsync(new CreateConversationLinkInput
            {
                SessionId = session.Id,
                LinkedEntityType = "HelpDesk.Ticket",
                LinkedEntityId = Guid.NewGuid().ToString("D")
            });

            tenantALinkId = link.Id;
        }

        using (CurrentTenant.Change(ChatTestData.TenantBId))
        {
            var links = await _conversationLinkAppService.GetBySessionAsync(tenantASessionId);
            links.ShouldBeEmpty();
        }

        using (CurrentTenant.Change(ChatTestData.TenantAId))
        {
            var links = await _conversationLinkAppService.GetBySessionAsync(tenantASessionId);
            links.ShouldContain(link => link.Id == tenantALinkId);
        }
    }
}
