using SufiChain.SufiAbp.AIManagement.Workspaces;
using Volo.Abp.DependencyInjection;

namespace SufiChain.Chat.AiUsage;

/// <summary>
/// Resolves assistant workspace names using session metadata, explicit overrides, and tenant defaults.
/// Validates resolved names against active AIManagement workspaces when possible.
/// </summary>
public class AIManagementChatAssistantWorkspaceResolver : IChatAssistantWorkspaceResolver, ITransientDependency
{
    protected IChatAiWorkspaceSelectionStore WorkspaceSelectionStore { get; }

    protected IWorkspaceRepository WorkspaceRepository { get; }

    public AIManagementChatAssistantWorkspaceResolver(
        IChatAiWorkspaceSelectionStore workspaceSelectionStore,
        IWorkspaceRepository workspaceRepository)
    {
        WorkspaceSelectionStore = workspaceSelectionStore;
        WorkspaceRepository = workspaceRepository;
    }

    public virtual async Task<string?> ResolveWorkspaceNameAsync(ChatAssistantWorkspaceResolveContext context)
    {
        var mappings = await WorkspaceSelectionStore.GetAssistantMappingsAsync();
        var assistantKey = context.AssistantKey ?? ChatAssistantMetadata.TryGetAssistantKey(context.SessionMetadataJson);

        var candidates = new[]
        {
            Normalize(context.ExplicitWorkspaceName),
            ChatAssistantMetadata.TryGetWorkspaceName(context.SessionMetadataJson),
            ChatAssistantMappings.TryResolveWorkspaceName(mappings, assistantKey),
            await WorkspaceSelectionStore.GetDefaultWorkspaceNameAsync()
        };

        foreach (var candidate in candidates)
        {
            if (string.IsNullOrWhiteSpace(candidate))
            {
                continue;
            }

            var workspace = await WorkspaceRepository.FindByNameAsync(candidate);
            if (workspace != null && workspace.IsActive && !string.IsNullOrWhiteSpace(workspace.Model))
            {
                return workspace.Name;
            }
        }

        return null;
    }

    protected static string? Normalize(string? workspaceName)
    {
        return string.IsNullOrWhiteSpace(workspaceName) ? null : workspaceName.Trim();
    }
}
