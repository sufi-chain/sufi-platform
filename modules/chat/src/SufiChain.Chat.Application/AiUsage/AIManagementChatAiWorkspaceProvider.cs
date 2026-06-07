using SufiChain.SufiAbp.AIManagement.Workspaces;
using Volo.Abp.DependencyInjection;

namespace SufiChain.Chat.AiUsage;

/// <summary>
/// Lists AIManagement workspaces for Chat settings and availability checks.
/// </summary>
public class AIManagementChatAiWorkspaceProvider : IChatAiWorkspaceProvider, ITransientDependency
{
    protected IWorkspaceRepository WorkspaceRepository { get; }

    public AIManagementChatAiWorkspaceProvider(IWorkspaceRepository workspaceRepository)
    {
        WorkspaceRepository = workspaceRepository;
    }

    public virtual Task<bool> IsIntegrationReadyAsync()
    {
        return Task.FromResult(true);
    }

    public virtual async Task<List<ChatAiWorkspaceOptionDto>> GetOptionsAsync()
    {
        var workspaces = await WorkspaceRepository.GetListAsync(
            maxResultCount: int.MaxValue,
            sorting: nameof(Workspace.Name));

        return workspaces
            .Where(IsChatCapable)
            .Select(workspace => new ChatAiWorkspaceOptionDto
            {
                Name = workspace.Name,
                DisplayName = workspace.Name,
                IsHealthy = IsHealthy(workspace)
            })
            .OrderBy(option => option.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public virtual async Task<bool> IsHealthyAsync(string workspaceName)
    {
        if (string.IsNullOrWhiteSpace(workspaceName))
        {
            return false;
        }

        var workspace = await WorkspaceRepository.FindByNameAsync(workspaceName.Trim());
        return workspace != null && IsHealthy(workspace);
    }

    protected static bool IsChatCapable(Workspace workspace)
    {
        return workspace.IsActive && !string.IsNullOrWhiteSpace(workspace.Model);
    }

    protected static bool IsHealthy(Workspace workspace)
    {
        return IsChatCapable(workspace) && !string.IsNullOrWhiteSpace(workspace.ApiKey);
    }
}
