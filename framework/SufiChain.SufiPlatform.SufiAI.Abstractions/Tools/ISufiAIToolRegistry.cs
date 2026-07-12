using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// Registry for discovering AI tools available in a workspace.
/// </summary>
public interface ISufiAIToolRegistry
{
    /// <summary>
    /// Gets all tools available for a specific workspace, respecting the
    /// provider's workspace configuration.
    /// </summary>
    Task<List<ISufiAITool>> GetToolsForWorkspaceAsync(
        string workspaceName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific tool by name for a workspace, or <c>null</c> when unknown.
    /// </summary>
    Task<ISufiAITool?> GetToolAsync(
        string workspaceName,
        string toolName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes the registry (re-scan published tools, reconnect external servers).
    /// </summary>
    Task RefreshAsync(CancellationToken cancellationToken = default);
}
