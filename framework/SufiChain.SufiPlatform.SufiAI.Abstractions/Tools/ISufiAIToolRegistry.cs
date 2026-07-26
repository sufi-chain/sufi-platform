using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// Tenant-aware registry for discovering and explicitly resolving AI tools.
/// </summary>
public interface ISufiAIToolRegistry
{
    /// <summary>
    /// Gets the tenant-visible tool catalog.
    /// </summary>
    Task<List<ISufiAITool>> GetCatalogAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves only explicitly requested tenant-visible tools.
    /// </summary>
    Task<List<ISufiAITool>> ResolveAsync(
        IReadOnlyCollection<string> toolNames,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes the registry (re-scan published tools, reconnect external servers).
    /// </summary>
    Task RefreshAsync(CancellationToken cancellationToken = default);
}
