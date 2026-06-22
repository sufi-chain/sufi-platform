using Microsoft.SemanticKernel;

namespace SufiChain.SufiAbp.AI.Workspaces;

/// <summary>
/// Provides access to Semantic Kernel instances configured for specific workspaces.
/// </summary>
public interface IWorkspaceAccessor
{
    /// <summary>
    /// Gets a configured Kernel instance for the specified workspace.
    /// </summary>
    Task<Kernel> GetKernelAsync(string workspaceName, CancellationToken cancellationToken = default);
}
