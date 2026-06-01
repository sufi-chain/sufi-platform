using SufiChain.Chat.Settings;
using SufiChain.SufiAbp.AI.Features;
using SufiChain.SufiAbp.Features;
using Volo.Abp;
using Volo.Abp.Settings;

namespace SufiChain.Chat.AiUsage;

public class ChatAssistantAvailabilityAppService : ChatAppService, IChatAssistantAvailabilityAppService
{
    protected new ISettingProvider SettingProvider { get; }
    protected IChatAiWorkspaceProvider WorkspaceProvider { get; }
    protected IChatAiWorkspaceSelectionStore WorkspaceSelectionStore { get; }
    protected new IFeatureChecker FeatureChecker { get; }

    public ChatAssistantAvailabilityAppService(
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

    public virtual async Task<ChatAssistantAvailabilityDto> GetAsync()
    {
        var requiredFeatures = new List<string>
        {
            SufiAbpAIFeatures.Enable,
            SufiAbpAIFeatures.Workspaces,
            SufiAbpAIFeatures.Chat
        };
        var enabledFeatures = new List<string>();

        if (!await SettingProvider.IsTrueAsync(ChatSettingNames.Ai.Enabled))
        {
            return Unavailable("ChatAiDisabled", "Chat:AiUnavailable", requiredFeatures);
        }

        foreach (var featureName in requiredFeatures)
        {
            if (!await FeatureChecker.IsEnabledAsync(featureName))
            {
                return Unavailable("AiFeatureDisabled", "Chat:AiUnavailable", requiredFeatures);
            }

            enabledFeatures.Add(featureName);
        }

        if (!await WorkspaceProvider.IsIntegrationReadyAsync())
        {
            return Unavailable("AiIntegrationUnavailable", "Chat:AiUnavailable", requiredFeatures);
        }

        var defaultWorkspaceName = await WorkspaceSelectionStore.GetDefaultWorkspaceNameAsync();
        if (string.IsNullOrWhiteSpace(defaultWorkspaceName))
        {
            return Unavailable("DefaultWorkspaceMissing", "Chat:AiUnavailable", requiredFeatures);
        }

        if (!await WorkspaceProvider.IsHealthyAsync(defaultWorkspaceName))
        {
            return Unavailable("DefaultWorkspaceUnhealthy", "Chat:AiUnavailable", requiredFeatures, defaultWorkspaceName);
        }

        return new ChatAssistantAvailabilityDto
        {
            IsAvailable = true,
            RequiredFeatures = requiredFeatures,
            EnabledFeatures = enabledFeatures,
            DefaultWorkspaceName = defaultWorkspaceName
        };
    }

    protected virtual ChatAssistantAvailabilityDto Unavailable(
        string reasonCode,
        string messageKey,
        List<string> requiredFeatures,
        string? defaultWorkspaceName = null)
    {
        return new ChatAssistantAvailabilityDto
        {
            IsAvailable = false,
            ReasonCode = reasonCode,
            MessageKey = messageKey,
            RequiredFeatures = requiredFeatures,
            DefaultWorkspaceName = defaultWorkspaceName
        };
    }
}
