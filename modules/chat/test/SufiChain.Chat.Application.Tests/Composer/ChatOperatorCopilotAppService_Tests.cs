using SufiChain.Chat.Composer;
using SufiChain.Chat.Settings;
using SufiChain.Chat.Sessions;
using SufiChain.Chat.Supports;
using SufiChain.SufiAbp.AI.Features;
using SufiChain.SufiAbp.SettingManagement;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace SufiChain.Chat.Composer;

public class ChatOperatorCopilotAppService_Tests : ChatApplicationTestBase<ChatApplicationTestModule>
{
    private readonly IChatOperatorCopilotAppService _copilotAppService;
    private readonly IChatSessionAppService _sessionAppService;
    private readonly ISettingManager _settingManager;
    private readonly ConfigurableAiService _aiService;
    private readonly ConfigurableFeatureChecker _featureChecker;

    public ChatOperatorCopilotAppService_Tests()
    {
        _copilotAppService = GetRequiredService<IChatOperatorCopilotAppService>();
        _sessionAppService = GetRequiredService<IChatSessionAppService>();
        _settingManager = GetRequiredService<ISettingManager>();
        _aiService = GetRequiredService<ConfigurableAiService>();
        _featureChecker = GetRequiredService<ConfigurableFeatureChecker>();
    }

    [Fact]
    public async Task Should_Return_Suggested_Text_When_Guard_Allows()
    {
        await ConfigureCopilotAsync();
        _aiService.ResponseContent = "Rewritten draft";

        var session = await CreateSupportSessionAsync();

        using (CurrentUser.Change(ChatTestData.UserAId))
        {
            var result = await _copilotAppService.AssistAsync(new ChatOperatorCopilotInput
            {
                SessionId = session.Id,
                DraftText = "hello customer",
                Operation = ChatOperatorCopilotOperation.Rewrite
            });

            result.SuggestedText.ShouldBe("Rewritten draft");
            result.WorkspaceName.ShouldBe(ChatTestData.DefaultWorkspaceName);
        }
    }

    [Fact]
    public async Task Should_Reject_When_Ai_Setting_Disabled()
    {
        await ConfigureCopilotAsync();
        await _settingManager.SetGlobalAsync(ChatSettingNames.Ai.Enabled, false.ToString());

        var session = await CreateSupportSessionAsync();

        using (CurrentUser.Change(ChatTestData.UserAId))
        {
            var exception = await Should.ThrowAsync<BusinessException>(async () =>
            {
                await _copilotAppService.AssistAsync(new ChatOperatorCopilotInput
                {
                    SessionId = session.Id,
                    DraftText = "hello customer",
                    Operation = ChatOperatorCopilotOperation.Rewrite
                });
            });

            exception.Code.ShouldBe(ChatErrorCodes.AiUnavailable);
        }
    }

    [Fact]
    public async Task Should_Reject_When_Draft_Is_Empty_For_Rewrite()
    {
        await ConfigureCopilotAsync();

        var session = await CreateSupportSessionAsync();

        using (CurrentUser.Change(ChatTestData.UserAId))
        {
            var exception = await Should.ThrowAsync<BusinessException>(async () =>
            {
                await _copilotAppService.AssistAsync(new ChatOperatorCopilotInput
                {
                    SessionId = session.Id,
                    DraftText = string.Empty,
                    Operation = ChatOperatorCopilotOperation.Rewrite
                });
            });

            exception.Code.ShouldBe(ChatErrorCodes.MessageContentRequired);
        }
    }

    private async Task ConfigureCopilotAsync()
    {
        await ChatTestSettingHelper.SetAiPolicyAsync(_settingManager);
        await ChatTestSettingHelper.SetDefaultWorkspaceAsync(_settingManager, ChatTestData.DefaultWorkspaceName);
        _featureChecker.Enable(
            SufiAbpAIFeatures.Enable,
            SufiAbpAIFeatures.Workspaces,
            SufiAbpAIFeatures.Chat);
    }

    private async Task<ChatSessionDto> CreateSupportSessionAsync()
    {
        using (CurrentUser.Change(ChatTestData.UserAId))
        {
            return await _sessionAppService.CreateAsync(new CreateChatSessionInput
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
        }
    }
}
