using Microsoft.AspNetCore.Authorization;
using SufiChain.Chat.Permissions;
using SufiChain.Chat.Settings;
using SufiChain.SufiAbp.AI.Features;
using SufiChain.SufiAbp.Features;
using Volo.Abp;
using Volo.Abp.Settings;

namespace SufiChain.Chat.AiUsage;

[Authorize(ChatPermissions.Settings.Manage)]
public class ChatAssistantConfigurationAppService : ChatAppService, IChatAssistantConfigurationAppService
{
    protected new ISettingProvider SettingProvider { get; }
    protected IChatAiWorkspaceProvider WorkspaceProvider { get; }
    protected IChatAiWorkspaceSelectionStore WorkspaceSelectionStore { get; }
    protected new IFeatureChecker FeatureChecker { get; }

    public ChatAssistantConfigurationAppService(
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

    public virtual async Task<ChatAssistantConfigurationDto> GetAsync()
    {
        var integrationReady = await IsIntegrationReadyAsync();
        var mappings = await WorkspaceSelectionStore.GetAssistantMappingsAsync();
        var workspaceOptions = integrationReady
            ? await WorkspaceProvider.GetOptionsAsync()
            : new List<ChatAiWorkspaceOptionDto>();
        var defaultWorkspaceName = await WorkspaceSelectionStore.GetDefaultWorkspaceNameAsync();

        return new ChatAssistantConfigurationDto
        {
            IsAvailable = integrationReady && await SettingProvider.IsTrueAsync(ChatSettingNames.Ai.Enabled),
            MessageKey = integrationReady ? null : "Chat:AiUnavailable",
            DefaultWorkspaceName = defaultWorkspaceName,
            WorkspaceOptions = workspaceOptions,
            Mappings = mappings
                .Select(item => MapMapping(item, workspaceOptions))
                .ToList()
        };
    }

    public virtual async Task UpdateAsync(UpdateChatAssistantConfigurationInput input)
    {
        if (!await IsIntegrationReadyAsync())
        {
            throw new BusinessException("Chat:AiIntegrationUnavailable");
        }

        var workspaceOptions = await WorkspaceProvider.GetOptionsAsync();
        var normalizedMappings = ValidateAndNormalizeMappings(input.Mappings, workspaceOptions);

        if (!string.IsNullOrWhiteSpace(input.DefaultWorkspaceName) &&
            workspaceOptions.All(option => !option.Name.Equals(input.DefaultWorkspaceName.Trim(), StringComparison.OrdinalIgnoreCase)))
        {
            throw new BusinessException("Chat:AssistantDefaultWorkspaceInvalid")
                .WithData("WorkspaceName", input.DefaultWorkspaceName);
        }

        await WorkspaceSelectionStore.SetDefaultWorkspaceNameAsync(input.DefaultWorkspaceName?.Trim());
        await WorkspaceSelectionStore.SetAssistantMappingsAsync(normalizedMappings);
    }

    protected virtual List<ChatAssistantMappingItem> ValidateAndNormalizeMappings(
        IReadOnlyList<ChatAssistantMappingDto> mappings,
        IReadOnlyList<ChatAiWorkspaceOptionDto> workspaceOptions)
    {
        if (mappings.Count > 32)
        {
            throw new BusinessException("Chat:AssistantMappingsLimitExceeded");
        }

        var normalized = new List<ChatAssistantMappingItem>();
        var seenKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var mapping in mappings)
        {
            var key = mapping.Key?.Trim().ToLowerInvariant() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(key))
            {
                continue;
            }

            if (!ChatAssistantMappings.IsValidKey(key))
            {
                throw new BusinessException("Chat:AssistantKeyInvalid")
                    .WithData("Key", key);
            }

            if (!seenKeys.Add(key))
            {
                throw new BusinessException("Chat:AssistantKeyDuplicate")
                    .WithData("Key", key);
            }

            var displayName = mapping.DisplayName?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(displayName))
            {
                throw new BusinessException("Chat:AssistantDisplayNameRequired")
                    .WithData("Key", key);
            }

            var workspaceName = mapping.WorkspaceName?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(workspaceName))
            {
                throw new BusinessException("Chat:AssistantWorkspaceRequired")
                    .WithData("Key", key);
            }

            if (workspaceOptions.All(option => !option.Name.Equals(workspaceName, StringComparison.OrdinalIgnoreCase)))
            {
                throw new BusinessException("Chat:AssistantWorkspaceInvalid")
                    .WithData("Key", key)
                    .WithData("WorkspaceName", workspaceName);
            }

            normalized.Add(new ChatAssistantMappingItem
            {
                Key = key,
                DisplayName = displayName,
                WorkspaceName = workspaceName,
                IsEnabled = mapping.IsEnabled,
                IsPublic = mapping.IsPublic
            });
        }

        return ChatAssistantMappings.Normalize(normalized).ToList();
    }

    protected virtual ChatAssistantMappingDto MapMapping(
        ChatAssistantMappingItem item,
        IReadOnlyList<ChatAiWorkspaceOptionDto> workspaceOptions)
    {
        var workspace = workspaceOptions.FirstOrDefault(option =>
            option.Name.Equals(item.WorkspaceName, StringComparison.OrdinalIgnoreCase));

        return new ChatAssistantMappingDto
        {
            Key = item.Key,
            DisplayName = item.DisplayName,
            WorkspaceName = item.WorkspaceName,
            IsEnabled = item.IsEnabled,
            IsPublic = item.IsPublic ?? true,
            IsWorkspaceHealthy = workspace?.IsHealthy == true
        };
    }

    protected virtual async Task<bool> IsIntegrationReadyAsync()
    {
        return await SettingProvider.IsTrueAsync(ChatSettingNames.Ai.Enabled) &&
               await FeatureChecker.IsEnabledAsync(SufiAbpAIFeatures.Enable) &&
               await FeatureChecker.IsEnabledAsync(SufiAbpAIFeatures.Workspaces) &&
               await FeatureChecker.IsEnabledAsync(SufiAbpAIFeatures.Chat) &&
               await WorkspaceProvider.IsIntegrationReadyAsync();
    }
}
