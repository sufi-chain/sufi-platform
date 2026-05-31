using System.Threading;
using System.Threading.Tasks;
using SufiChain.SufiAbp.AI.Features;
using SufiChain.SufiAbp.AIManagement.Workspaces;
using Volo.Abp.Application.Services;
using SufiChain.SufiAbp.Features;

namespace SufiChain.SufiAbp.AIManagement.AI;

/// <summary>
/// Application service that provides access to AI kernels.
/// Acts as a bridge between HttpApi and Domain layer.
/// </summary>
[RequiresFeature(SufiAbpAIFeatures.Enable)]
public class AIKernelAppService : ApplicationService, IAIKernelAppService
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
