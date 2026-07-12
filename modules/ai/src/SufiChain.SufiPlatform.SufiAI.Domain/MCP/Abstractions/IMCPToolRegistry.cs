using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;

/// <summary>
/// Registry for discovering and accessing MCP tools available in a workspace.
/// Merges internal tools (ApplicationService methods) and external tools (MCP servers).
/// </summary>
public interface IMCPToolRegistry
{
    /// <summary>
    /// Get all tools available for a specific workspace.
    /// Respects workspace configuration (enabled tools, allowed modules, external servers).
    /// </summary>
    Task<List<IMCPTool>> GetToolsForWorkspaceAsync(
        string workspaceName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all discovered tools for a specific workspace, ignoring workspace enabled-tool selection.
    /// Used by configuration UI.
    /// </summary>
    Task<List<IMCPTool>> GetAllToolsForWorkspaceAsync(
        string workspaceName,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get a specific tool by name for a workspace.
    /// </summary>
    Task<IMCPTool?> GetToolAsync(
        string workspaceName,
        string toolName,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Refresh the tool registry (re-scan internal tools, reconnect external servers).
    /// </summary>
    Task RefreshAsync(CancellationToken cancellationToken = default);
}
