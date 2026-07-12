using System.Threading;
using System.Threading.Tasks;
using SufiChain.SufiAbp.AI.Features;
using SufiChain.SufiAbp.AI.Workspaces;
using SufiChain.SufiAbp.Application.Services;
using SufiChain.SufiAbp.Features;

namespace SufiChain.SufiAbp.AI;

/// <summary>
/// Application service that provides access to AI kernels.
/// Acts as a bridge between HttpApi and Domain layer.
/// </summary>
[RequiresFeature(SufiAIFeatures.Enable)]
public class AIKernelAppService : SufiAbpApplicationService, IAIKernelAppService
{
    private readonly IWorkspaceAccessor _workspaceAccessor;

    public AIKernelAppService(IWorkspaceAccessor workspaceAccessor)
    {
        _workspaceAccessor = workspaceAccessor;
    }

    /// <summary>
    /// Gets a Semantic Kernel instance for the specified workspace.
    /// Returns as object to avoid exposing SK types in Application.Contracts.
    /// </summary>
    public async Task<object> GetKernelAsync(string workspaceName, CancellationToken cancellationToken = default)
    {
        return await _workspaceAccessor.GetKernelAsync(workspaceName, cancellationToken);
    }
}
