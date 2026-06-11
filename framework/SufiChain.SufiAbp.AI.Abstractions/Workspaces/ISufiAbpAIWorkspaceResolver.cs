using System.Threading;
using System.Threading.Tasks;

namespace SufiChain.SufiAbp.AI;

/// <summary>
/// Resolves a usable AI workspace for execution.
/// </summary>
public interface ISufiAbpAIWorkspaceResolver
{
    /// <summary>
    /// Resolves an active, ready workspace. When <paramref name="preferredWorkspaceName"/>
    /// is provided, it is validated and returned if usable; otherwise the provider's
    /// default workspace (if any) is returned. Returns <c>null</c> when nothing usable exists.
    /// </summary>
    Task<SufiAbpAIWorkspaceDescriptor?> ResolveAsync(
        string? preferredWorkspaceName = null,
        CancellationToken cancellationToken = default);
}
