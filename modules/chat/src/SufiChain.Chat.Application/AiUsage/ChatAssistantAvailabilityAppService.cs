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
        var assistants = await BuildAssistantPickerOptionsAsync(defaultWorkspaceName);

        if (assistants.Count > 0)
        {
            return new ChatAssistantAvailabilityDto
            {
                IsAvailable = true,
                RequiredFeatures = requiredFeatures,
                EnabledFeatures = enabledFeatures,
                DefaultWorkspaceName = defaultWorkspaceName,
                Assistants = assistants
            };
        }

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
            DefaultWorkspaceName = defaultWorkspaceName,
            Assistants = new List<ChatAssistantPickerOptionDto>()
        };
    }

    protected virtual async Task<List<ChatAssistantPickerOptionDto>> BuildAssistantPickerOptionsAsync(string? defaultWorkspaceName)
    {
        var mappings = await WorkspaceSelectionStore.GetAssistantMappingsAsync();
        var enabledMappings = mappings
            .Where(ChatAssistantMappings.IsMessengerVisible)
            .Where(item => !string.IsNullOrWhiteSpace(item.Key))
            .ToList();

        if (enabledMappings.Count == 0)
        {
            return new List<ChatAssistantPickerOptionDto>();
        }

        var options = new List<ChatAssistantPickerOptionDto>();
        foreach (var mapping in enabledMappings)
        {
            if (!await WorkspaceProvider.IsHealthyAsync(mapping.WorkspaceName))
            {
                continue;
            }

            options.Add(new ChatAssistantPickerOptionDto
            {
                Key = mapping.Key,
                DisplayName = mapping.DisplayName,
                WorkspaceName = mapping.WorkspaceName,
                IsDefault = !string.IsNullOrWhiteSpace(defaultWorkspaceName) &&
                            mapping.WorkspaceName.Equals(defaultWorkspaceName, StringComparison.OrdinalIgnoreCase)
            });
        }

        return options
            .OrderByDescending(item => item.IsDefault)
            .ThenBy(item => item.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToList();
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
