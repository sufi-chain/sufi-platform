using SufiChain.SufiPlatform.SufiCom.Chat.Composer;
using SufiChain.SufiPlatform.SufiCom.Chat.Settings;
using SufiChain.SufiPlatform.SufiCom.Chat.Sessions;
using SufiChain.SufiPlatform.SufiCom.Chat.Supports;
using SufiChain.SufiPlatform.SufiAI.Features;
using SufiChain.SufiPlatform.Settings;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Chat.Composer;

public class ChatOperatorHooshvareAppService_Tests : ChatApplicationTestBase<SufiComChatApplicationTestModule>
{
    private readonly IChatOperatorHooshvareAppService _hooshvareAppService;
    private readonly IChatSessionAppService _sessionAppService;
    private readonly ISettingManager _settingManager;
    private readonly ConfigurableAiService _aiService;
    private readonly ConfigurableFeatureChecker _featureChecker;

    public ChatOperatorHooshvareAppService_Tests()
    {
        _hooshvareAppService = GetRequiredService<IChatOperatorHooshvareAppService>();
        _sessionAppService = GetRequiredService<IChatSessionAppService>();
        _settingManager = GetRequiredService<ISettingManager>();
        _aiService = GetRequiredService<ConfigurableAiService>();
        _featureChecker = GetRequiredService<ConfigurableFeatureChecker>();
    }

    [Fact]
    public async Task Should_Return_Suggested_Text_When_Guard_Allows()
    {
        await ConfigureHooshvareAsync();
        _aiService.ResponseContent = "Rewritten draft";

        var session = await CreateSupportSessionAsync();

        using (CurrentUser.Change(ChatTestData.UserAId))
        {
            var result = await _hooshvareAppService.AssistAsync(new ChatOperatorHooshvareInput
            {
                SessionId = session.Id,
                DraftText = "hello customer",
                Operation = ChatOperatorHooshvareOperation.Rewrite
            });

            result.SuggestedText.ShouldBe("Rewritten draft");
            result.WorkspaceName.ShouldBe(ChatTestData.DefaultWorkspaceName);
        }
    }

    [Fact]
    public async Task Should_Reject_When_Ai_Setting_Disabled()
    {
        await ConfigureHooshvareAsync();
        await _settingManager.SetGlobalAsync(ChatSettingNames.Ai.Enabled, false.ToString());

        var session = await CreateSupportSessionAsync();

        using (CurrentUser.Change(ChatTestData.UserAId))
        {
            var exception = await Should.ThrowAsync<Volo.Abp.BusinessException>(async () =>
            {
                await _hooshvareAppService.AssistAsync(new ChatOperatorHooshvareInput
                {
                    SessionId = session.Id,
                    DraftText = "hello customer",
                    Operation = ChatOperatorHooshvareOperation.Rewrite
                });
            });

            exception.Code.ShouldBe(ChatErrorCodes.AiUnavailable);
        }
    }

    [Fact]
    public async Task Should_Reject_When_Draft_Is_Empty_For_Rewrite()
    {
        await ConfigureHooshvareAsync();

        var session = await CreateSupportSessionAsync();

        using (CurrentUser.Change(ChatTestData.UserAId))
        {
            var exception = await Should.ThrowAsync<Volo.Abp.BusinessException>(async () =>
            {
                await _hooshvareAppService.AssistAsync(new ChatOperatorHooshvareInput
                {
                    SessionId = session.Id,
                    DraftText = string.Empty,
                    Operation = ChatOperatorHooshvareOperation.Rewrite
                });
            });

            exception.Code.ShouldBe(ChatErrorCodes.MessageContentRequired);
        }
    }

    private async Task ConfigureHooshvareAsync()
    {
        await ChatTestSettingHelper.SetAiPolicyAsync(_settingManager);
        await ChatTestSettingHelper.SetDefaultWorkspaceAsync(_settingManager, ChatTestData.DefaultWorkspaceName);
        _featureChecker.Enable(
            SufiAIFeatures.Enable,
            SufiAIFeatures.Workspaces,
            SufiAIFeatures.Chat);
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
