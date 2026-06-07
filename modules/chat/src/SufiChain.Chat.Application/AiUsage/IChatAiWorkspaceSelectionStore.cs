using SufiChain.Chat.Settings;
using SufiChain.SufiAbp.SettingManagement;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Settings;

namespace SufiChain.Chat.AiUsage;

public interface IChatAiWorkspaceSelectionStore
{
    Task<string?> GetDefaultWorkspaceNameAsync();

    Task SetDefaultWorkspaceNameAsync(string? workspaceName);

    Task<IReadOnlyList<ChatAssistantMappingItem>> GetAssistantMappingsAsync();

    Task SetAssistantMappingsAsync(IReadOnlyList<ChatAssistantMappingItem> mappings);
}

public class NullChatAiWorkspaceSelectionStore : IChatAiWorkspaceSelectionStore, ITransientDependency
{
    protected ISettingProvider SettingProvider { get; }
    protected ISettingManager SettingManager { get; }
    protected ICurrentTenant CurrentTenant { get; }

    public NullChatAiWorkspaceSelectionStore(
        ISettingProvider settingProvider,
        ISettingManager settingManager,
        ICurrentTenant currentTenant)
    {
        SettingProvider = settingProvider;
        SettingManager = settingManager;
        CurrentTenant = currentTenant;
    }

    public virtual Task<string?> GetDefaultWorkspaceNameAsync()
    {
        return SettingProvider.GetOrNullAsync(ChatSettingNames.Ai.DefaultWorkspaceName);
    }

    public virtual Task SetDefaultWorkspaceNameAsync(string? workspaceName)
    {
        return SettingManager.SetForTenantOrGlobalAsync(
            CurrentTenant.Id,
            ChatSettingNames.Ai.DefaultWorkspaceName,
            workspaceName);
    }

    public virtual async Task<IReadOnlyList<ChatAssistantMappingItem>> GetAssistantMappingsAsync()
    {
        var json = await SettingProvider.GetOrNullAsync(ChatSettingNames.Ai.AssistantMappings);
        return ChatAssistantMappings.Parse(json);
    }

    public virtual Task SetAssistantMappingsAsync(IReadOnlyList<ChatAssistantMappingItem> mappings)
    {
        var normalized = ChatAssistantMappings.Normalize(mappings);
        return SettingManager.SetForTenantOrGlobalAsync(
            CurrentTenant.Id,
            ChatSettingNames.Ai.AssistantMappings,
            ChatAssistantMappings.Serialize(normalized));
    }
}
