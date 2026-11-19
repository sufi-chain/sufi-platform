using SufiChain.SufiPlatform.Settings;
using SufiChain.SufiPlatform.SufiAI.Features;
using SufiChain.SufiPlatform.SufiAI.Hooshvare;
using SufiChain.SufiPlatform.SufiAI.Hooshvare.Agents;
using SufiChain.SufiPlatform.SufiAI.Hooshvare.Features;
using SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;
using SufiChain.SufiPlatform.SufiCom.Chat.Application.Messages;
using SufiChain.SufiPlatform.SufiCom.Chat.ETOs;
using SufiChain.SufiPlatform.SufiCom.Chat.Usage;
using SufiChain.SufiPlatform.SufiCom.Chat.Hooshvare;
using SufiChain.SufiPlatform.SufiCom.Chat.Messages;
using SufiChain.SufiPlatform.SufiCom.Chat.Participants;
using SufiChain.SufiPlatform.SufiCom.Chat.Sessions;
using SufiChain.SufiPlatform.SufiCom.Chat.Settings;
using SufiChain.SufiPlatform.SufiCom.Chat.Supports;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Chat.AiUsage;

/// <summary>
/// Free editions leave <see cref="SufiAiHooshvaresFeatures.Agents"/> off. That is the existing
/// edition feature, not a new plan. The Everyday Assistant stays. Other agents do not.
/// </summary>
public class ChatAgentPlanEntitlement_Tests : ChatApplicationTestBase<SufiComChatApplicationTestModule>
{
    private static readonly Guid HomeAgentId = Guid.Parse("c0a10000-0000-4000-8000-000000000010");

    private readonly IChatAssistantAvailabilityAppService _availability;
    private readonly IChatAgentOrchestratorAppService _orchestrator;
    private readonly IChatSessionAppService _sessions;
    private readonly IChatMessageRepository _messages;
    private readonly ISettingManager _settings;
    private readonly ConfigurableFeatureChecker _features;
    private readonly ConfigurableChatAiWorkspaceProvider _workspaces;
    private readonly TestHooshvareCatalogAppService _catalog;
    private readonly ChatAssistantMessageHandler _handler;
    private readonly ConfigurableAiService _ai;

    public ChatAgentPlanEntitlement_Tests()
    {
        _availability = GetRequiredService<IChatAssistantAvailabilityAppService>();
        _orchestrator = GetRequiredService<IChatAgentOrchestratorAppService>();
        _sessions = GetRequiredService<IChatSessionAppService>();
        _messages = GetRequiredService<IChatMessageRepository>();
        _settings = GetRequiredService<ISettingManager>();
        _features = GetRequiredService<ConfigurableFeatureChecker>();
        _workspaces = GetRequiredService<ConfigurableChatAiWorkspaceProvider>();
        _catalog = GetRequiredService<TestHooshvareCatalogAppService>();
        _handler = GetRequiredService<ChatAssistantMessageHandler>();
        _ai = GetRequiredService<ConfigurableAiService>();
    }

    [Fact]
    public async Task Free_edition_sees_and_runs_only_the_everyday_assistant()
    {
        _catalog.Add(HomeAgent());
        _features.Disable(SufiAiHooshvaresFeatures.Agents);
        var previousContent = _ai.ResponseContent;
        _ai.ResponseContent = "everyday reply";
        try
        {
            await PrepareAssistantAsync();
            await GetRequiredService<IChatAssistantAvailabilityCache>().InvalidateAsync(null, hostWide: true);

            ChatAssistantAvailabilityDto hidden;
            using (CurrentUser.Change(ChatTestData.UserAId))
            {
                hidden = await _availability.GetAsync();
            }
            hidden.IsAvailable.ShouldBeTrue();
            hidden.Assistants.ShouldNotBeEmpty();
            hidden.Assistants.ShouldAllBe(option => option.Key == ChatHooshvareKeys.PublicAssistant.Key);
            hidden.Assistants.ShouldAllBe(option => option.DisplayName == "Everyday Assistant");
            hidden.Assistants.ShouldNotContain(option => option.Id == HomeAgentId);

            var session = await CreateAssistantSessionAsync();
            var userMessage = await InsertVisitorMessageAsync(session.Id, "hello");
            await _handler.ProcessAsync(new ChatMessageSentEto
            {
                Id = Guid.Empty,
                SessionId = session.Id,
                MessageId = userMessage.Id,
                SenderKind = ChatMessageSenderKind.Visitor,
                SentAt = DateTime.UtcNow
            });

            var stored = await _messages.GetListBySessionAsync(session.Id, includeInternal: false);
            stored.ShouldContain(message =>
                message.SenderKind == ChatMessageSenderKind.Assistant && message.Body == "everyday reply");

            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _orchestrator.HandleUserMessageAsync(new ChatAgentUserTurnInput
                {
                    SessionId = session.Id,
                    UserMessageId = userMessage.Id,
                    Text = "use the cms agent",
                    Culture = "fa"
                }));
            exception.Code.ShouldBe(AIHooshvareErrorCodes.AgentNotInPlan);
        }
        finally
        {
            _ai.ResponseContent = previousContent;
            _features.Enable(SufiAiHooshvaresFeatures.Agents);
            _catalog.Remove(HomeAgentId);
        }
    }

    [Fact]
    public async Task An_over_quota_tenant_cannot_start_an_agent_turn()
    {
        _features.Enable(SufiAiHooshvaresFeatures.Agents);
        await ChatTestSettingHelper.SetAiPolicyAsync(_settings);
        var tenantId = Guid.NewGuid();
        await _settings.SetForTenantAsync(tenantId, ChatSettingNames.Ai.MaxTokensPerTenantPerDay, "5");
        var guard = GetRequiredService<IChatUsageGuard>();
        var sessionId = Guid.NewGuid();
        using var tenant = CurrentTenant.Change(tenantId);
        try
        {
            var reservationId = await guard.ReserveAiUsageAsync(sessionId, ChatAiOperationKind.AutoReply);
            await guard.RecordAiUsageAsync(reservationId, new ChatAiUsageRecord
            {
                TotalTokens = 5,
                PromptTokens = 2,
                CompletionTokens = 3
            });

            var exception = await Should.ThrowAsync<BusinessException>(() =>
                _orchestrator.HandleUserMessageAsync(new ChatAgentUserTurnInput
                {
                    SessionId = Guid.NewGuid(),
                    UserMessageId = Guid.NewGuid(),
                    Text = "hello",
                    Culture = "en"
                }));
            exception.Code.ShouldBe(ChatErrorCodes.AiTokenLimitExceeded);
        }
        finally
        {
            await _settings.SetForTenantAsync(tenantId, ChatSettingNames.Ai.MaxTokensPerTenantPerDay, null);
        }
    }

    private async Task PrepareAssistantAsync()
    {
        await ChatTestSettingHelper.SetAiPolicyAsync(_settings);
        await _settings.SetGlobalAsync(ChatSettingNames.Ai.InboxAssistantKeys, ChatInboxAssistantKeys.DefaultJson);
        _features.Enable(SufiAIFeatures.Enable, SufiAIFeatures.Workspaces, SufiAIFeatures.Chat);
        _workspaces.IntegrationReady = true;
        _workspaces.HealthyWorkspaces.Clear();
        _workspaces.HealthyWorkspaceIds.Clear();
        _workspaces.HealthyWorkspaces.Add(ChatTestData.DefaultWorkspaceName);
        _workspaces.HealthyWorkspaceIds.Add(TestHooshvareCatalogAppService.PublicAssistantWorkspaceId);
    }

    private static HooshvareCatalogItemDto HomeAgent()
    {
        return new HooshvareCatalogItemDto
        {
            Id = HomeAgentId,
            Key = ChatHooshvareKeys.HomeAgent.Key,
            SourceModule = "SufiCom.Chat",
            DisplayName = "Home Agent",
            WorkspaceId = TestHooshvareCatalogAppService.PublicAssistantWorkspaceId,
            WorkspaceName = ChatTestData.DefaultWorkspaceName,
            Kind = HooshvareKind.Agent,
            Purpose = "HomeAgent",
            IsPublic = false,
            RequiredFeatureName = SufiAiHooshvaresFeatures.Agents
        };
    }

    private async Task<ChatSessionDto> CreateAssistantSessionAsync()
    {
        using (CurrentUser.Change(ChatTestData.UserAId))
        {
            return await _sessions.CreateAsync(new CreateChatSessionInput
            {
                Title = "Everyday",
                AccessMode = AccessMode.Internal,
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

    private async Task<ChatMessage> InsertVisitorMessageAsync(Guid sessionId, string body)
    {
        var message = new ChatMessage(
            Guid.NewGuid(),
            CurrentTenant.Id,
            sessionId,
            body,
            ChatMessageSenderKind.Visitor);
        return await _messages.InsertAsync(message, autoSave: true);
    }
}
