using System.Threading;
using System.Threading.Tasks;

namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// Resolves a usable AI workspace for execution.
/// </summary>
public interface ISufiAIWorkspaceResolver
{
    /// <summary>
    /// Resolves an active, ready workspace. When <paramref name="preferredWorkspaceName"/>
    /// is provided, it is validated and returned if usable; otherwise the provider's
    /// default workspace (if any) is returned. Returns <c>null</c> when nothing usable exists.
    /// </summary>
    Task<SufiAIWorkspaceDescriptor?> ResolveAsync(
        string? preferredWorkspaceName = null,
        CancellationToken cancellationToken = default);
}
