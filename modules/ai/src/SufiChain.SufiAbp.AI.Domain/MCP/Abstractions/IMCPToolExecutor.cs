using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SufiChain.SufiAbp.AI.MCP.Abstractions;

/// <summary>
/// Executes MCP tools with proper context, validation, and auditing.
/// </summary>
public interface IMCPToolExecutor
{
    /// <summary>
    /// Execute a tool by name with parameters in a workspace context.
    /// </summary>
    Task<MCPToolExecutionResult> ExecuteAsync(
        string workspaceName,
        string toolName,
        Dictionary<string, object?> parameters,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Execute a tool instance directly.
    /// </summary>
    Task<MCPToolExecutionResult> ExecuteAsync(
        IMCPTool tool,
        WorkspaceContext context,
        Dictionary<string, object?> parameters,
        CancellationToken cancellationToken = default);
}
