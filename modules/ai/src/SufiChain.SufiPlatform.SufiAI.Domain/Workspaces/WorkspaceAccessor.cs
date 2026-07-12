using Microsoft.SemanticKernel;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.SufiAI.Workspaces;

public class WorkspaceAccessor : IWorkspaceAccessor, ITransientDependency
{
    private readonly WorkspaceSyncService _syncService;
    
    public WorkspaceAccessor(WorkspaceSyncService syncService)
    {
        _syncService = syncService;
    }
    
    public async Task<Kernel> GetKernelAsync(string workspaceName, CancellationToken cancellationToken = default)
    {
        return await _syncService.GetOrCreateKernelAsync(workspaceName, cancellationToken);
    }
}
