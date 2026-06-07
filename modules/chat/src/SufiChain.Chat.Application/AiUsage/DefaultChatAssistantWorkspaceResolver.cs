using Volo.Abp.DependencyInjection;

namespace SufiChain.Chat.AiUsage;

/// <summary>
/// Falls back to the tenant default workspace from Chat settings.
/// </summary>
public class DefaultChatAssistantWorkspaceResolver : IChatAssistantWorkspaceResolver, ITransientDependency
{
    protected IChatAiWorkspaceSelectionStore WorkspaceSelectionStore { get; }

    public DefaultChatAssistantWorkspaceResolver(IChatAiWorkspaceSelectionStore workspaceSelectionStore)
    {
        WorkspaceSelectionStore = workspaceSelectionStore;
    }

    public virtual async Task<string?> ResolveWorkspaceNameAsync(ChatAssistantWorkspaceResolveContext context)
    {
        if (!string.IsNullOrWhiteSpace(context.ExplicitWorkspaceName))
        {
            return context.ExplicitWorkspaceName.Trim();
        }

        var metadataWorkspace = ChatAssistantMetadata.TryGetWorkspaceName(context.SessionMetadataJson);
        if (!string.IsNullOrWhiteSpace(metadataWorkspace))
        {
            return metadataWorkspace;
        }

        var mappings = await WorkspaceSelectionStore.GetAssistantMappingsAsync();
        var assistantKey = context.AssistantKey ?? ChatAssistantMetadata.TryGetAssistantKey(context.SessionMetadataJson);
        var mappedWorkspace = ChatAssistantMappings.TryResolveWorkspaceName(mappings, assistantKey);
        if (!string.IsNullOrWhiteSpace(mappedWorkspace))
        {
            return mappedWorkspace;
        }

        return await WorkspaceSelectionStore.GetDefaultWorkspaceNameAsync();
    }
}
