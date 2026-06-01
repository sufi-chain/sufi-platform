using SufiChain.Chat.AiUsage;
using SufiChain.Chat.Settings;
using SufiChain.Chat.Supports;
using SufiChain.SufiAbp.AI.Features;
using SufiChain.SufiAbp.SettingManagement;
using Shouldly;
using Xunit;

namespace SufiChain.Chat.AiUsage;

public class ChatAssistantAvailabilityAppService_Tests : ChatApplicationTestBase<ChatApplicationTestModule>
{
    private readonly IChatAssistantAvailabilityAppService _assistantAvailabilityAppService;
    private readonly ISettingManager _settingManager;
    private readonly ConfigurableFeatureChecker _featureChecker;
    private readonly ConfigurableChatAiWorkspaceProvider _workspaceProvider;

    public ChatAssistantAvailabilityAppService_Tests()
    {
        _assistantAvailabilityAppService = GetRequiredService<IChatAssistantAvailabilityAppService>();
        _settingManager = GetRequiredService<ISettingManager>();
        _featureChecker = GetRequiredService<ConfigurableFeatureChecker>();
        _workspaceProvider = GetRequiredService<ConfigurableChatAiWorkspaceProvider>();
    }

    [Fact]
    public async Task Should_Be_Available_When_All_Checks_Pass()
    {
        await ConfigureAvailableAssistantAsync();

        var availability = await _assistantAvailabilityAppService.GetAsync();

        availability.IsAvailable.ShouldBeTrue();
        availability.DefaultWorkspaceName.ShouldBe(ChatTestData.DefaultWorkspaceName);
        availability.EnabledFeatures.ShouldContain(SufiAbpAIFeatures.Enable);
        availability.EnabledFeatures.ShouldContain(SufiAbpAIFeatures.Workspaces);
        availability.EnabledFeatures.ShouldContain(SufiAbpAIFeatures.Chat);
    }

    [Fact]
    public async Task Should_Be_Unavailable_When_Chat_Ai_Setting_Disabled()
    {
        await ConfigureAvailableAssistantAsync();
        await _settingManager.SetGlobalAsync(ChatSettingNames.Ai.Enabled, false.ToString());

        var availability = await _assistantAvailabilityAppService.GetAsync();

        availability.IsAvailable.ShouldBeFalse();
        availability.ReasonCode.ShouldBe("ChatAiDisabled");
    }

    [Fact]
    public async Task Should_Be_Unavailable_When_SufiAbpAIFeatures_Enable_Is_Disabled()
    {
        await ConfigureAvailableAssistantAsync();
        _featureChecker.Disable(SufiAbpAIFeatures.Enable);

        var availability = await _assistantAvailabilityAppService.GetAsync();

        availability.IsAvailable.ShouldBeFalse();
        availability.ReasonCode.ShouldBe("AiFeatureDisabled");
    }

    [Fact]
    public async Task Should_Be_Unavailable_When_Workspaces_Feature_Is_Disabled()
    {
        await ConfigureAvailableAssistantAsync();
        _featureChecker.Disable(SufiAbpAIFeatures.Workspaces);

        var availability = await _assistantAvailabilityAppService.GetAsync();

        availability.IsAvailable.ShouldBeFalse();
        availability.ReasonCode.ShouldBe("AiFeatureDisabled");
    }

    [Fact]
    public async Task Should_Be_Unavailable_When_Chat_Feature_Is_Disabled()
    {
        await ConfigureAvailableAssistantAsync();
        _featureChecker.Disable(SufiAbpAIFeatures.Chat);

        var availability = await _assistantAvailabilityAppService.GetAsync();

        availability.IsAvailable.ShouldBeFalse();
        availability.ReasonCode.ShouldBe("AiFeatureDisabled");
    }

    [Fact]
    public async Task Should_Be_Unavailable_When_Integration_Is_Not_Ready()
    {
        await ConfigureAvailableAssistantAsync();
        _workspaceProvider.IntegrationReady = false;

        var availability = await _assistantAvailabilityAppService.GetAsync();

        availability.IsAvailable.ShouldBeFalse();
        availability.ReasonCode.ShouldBe("AiIntegrationUnavailable");
    }

    [Fact]
    public async Task Should_Be_Unavailable_When_Default_Workspace_Is_Missing()
    {
        await ChatTestSettingHelper.SetAiPolicyAsync(_settingManager);
        _workspaceProvider.IntegrationReady = true;
        await _settingManager.SetGlobalAsync(ChatSettingNames.Ai.DefaultWorkspaceName, string.Empty);

        var availability = await _assistantAvailabilityAppService.GetAsync();

        availability.IsAvailable.ShouldBeFalse();
        availability.ReasonCode.ShouldBe("DefaultWorkspaceMissing");
    }

    [Fact]
    public async Task Should_Be_Unavailable_When_Default_Workspace_Is_Unhealthy()
    {
        await ConfigureAvailableAssistantAsync();
        _workspaceProvider.HealthyWorkspaces.Clear();

        var availability = await _assistantAvailabilityAppService.GetAsync();

        availability.IsAvailable.ShouldBeFalse();
        availability.ReasonCode.ShouldBe("DefaultWorkspaceUnhealthy");
        availability.DefaultWorkspaceName.ShouldBe(ChatTestData.DefaultWorkspaceName);
    }

    private async Task ConfigureAvailableAssistantAsync()
    {
        await ChatTestSettingHelper.SetAiPolicyAsync(_settingManager);
        await ChatTestSettingHelper.SetDefaultWorkspaceAsync(_settingManager, ChatTestData.DefaultWorkspaceName);
        _featureChecker.Enable(
            SufiAbpAIFeatures.Enable,
            SufiAbpAIFeatures.Workspaces,
            SufiAbpAIFeatures.Chat);
        _workspaceProvider.IntegrationReady = true;
        _workspaceProvider.HealthyWorkspaces.Clear();
        _workspaceProvider.HealthyWorkspaces.Add(ChatTestData.DefaultWorkspaceName);
    }
}
