using Microsoft.AspNetCore.Authorization;
using SufiChain.Chat.Permissions;
using SufiChain.Chat.Settings;
using SufiChain.SufiAbp.AI.Features;
using SufiChain.SufiAbp.Features;
using Volo.Abp;
using Volo.Abp.Settings;

namespace SufiChain.Chat.AiUsage;

[Authorize(ChatPermissions.Settings.Manage)]
public class ChatAiWorkspaceSelectionAppService : ChatAppService, IChatAiWorkspaceSelectionAppService
{
    protected new ISettingProvider SettingProvider { get; }
    protected IChatAiWorkspaceProvider WorkspaceProvider { get; }
    protected IChatAiWorkspaceSelectionStore WorkspaceSelectionStore { get; }
    protected new IFeatureChecker FeatureChecker { get; }

    public ChatAiWorkspaceSelectionAppService(
        ISettingProvider settingProvider,
        IChatAiWorkspaceProvider workspaceProvider,
        IChatAiWorkspaceSelectionStore workspaceSelectionStore,
        IFeatureChecker featureChecker)
    {
        SettingProvider = settingProvider;
        WorkspaceProvider = workspaceProvider;
        WorkspaceSelectionStore = workspaceSelectionStore;
        FeatureChecker = featureChecker;
    }

    public virtual async Task<ChatAiWorkspaceSelectionDto> GetAsync()
    {
        var defaultWorkspaceName = await WorkspaceSelectionStore.GetDefaultWorkspaceNameAsync();
        var aiEnabled = await SettingProvider.IsTrueAsync(ChatSettingNames.Ai.Enabled);
        var requiredFeaturesEnabled = await AreRequiredFeaturesEnabledAsync();
        var integrationReady = requiredFeaturesEnabled && await WorkspaceProvider.IsIntegrationReadyAsync();

        return new ChatAiWorkspaceSelectionDto
        {
            IsAvailable = aiEnabled && integrationReady,
            ReasonCode = aiEnabled
                ? integrationReady ? null : requiredFeaturesEnabled ? "AiIntegrationUnavailable" : "AiFeatureDisabled"
                : "ChatAiDisabled",
            MessageKey = aiEnabled && integrationReady ? null : "Chat:AiUnavailable",
            DefaultWorkspaceName = defaultWorkspaceName
        };
    }

    public virtual async Task<List<ChatAiWorkspaceOptionDto>> GetOptionsAsync()
    {
        if (!await AreRequiredFeaturesEnabledAsync())
        {
            return new List<ChatAiWorkspaceOptionDto>();
        }

        var options = await WorkspaceProvider.GetOptionsAsync();
        var defaultWorkspaceName = await WorkspaceSelectionStore.GetDefaultWorkspaceNameAsync();

        foreach (var option in options)
        {
            option.IsDefault = !string.IsNullOrWhiteSpace(defaultWorkspaceName) &&
                               option.Name.Equals(defaultWorkspaceName, StringComparison.OrdinalIgnoreCase);
        }

        return options;
    }

    public virtual async Task UpdateDefaultAsync(UpdateChatAiWorkspaceSelectionInput input)
    {
        await WorkspaceSelectionStore.SetDefaultWorkspaceNameAsync(input.DefaultWorkspaceName);
    }

    protected virtual async Task<bool> AreRequiredFeaturesEnabledAsync()
    {
        return await FeatureChecker.IsEnabledAsync(SufiAbpAIFeatures.Enable) &&
               await FeatureChecker.IsEnabledAsync(SufiAbpAIFeatures.Workspaces) &&
               await FeatureChecker.IsEnabledAsync(SufiAbpAIFeatures.Chat);
    }
}
