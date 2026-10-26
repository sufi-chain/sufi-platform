using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;

/// <summary>
/// Executes MCP tools with proper context, validation, and auditing.
/// </summary>
public interface IMCPToolExecutor
{
    /// <summary>
    /// Execute a tool by name with parameters in a workspace context.
    /// </summary>
    /// <param name="sessionId">
    /// sufi-chain#384 slice 6b: conversation the call belongs to, when the caller knows it. It is carried on
    /// the bound call context so audit rows and per-user external MCP tokens name the conversation.
    /// </param>
    Task<MCPToolExecutionResult> ExecuteAsync(
        string workspaceName,
        string toolName,
        Dictionary<string, object?> parameters,
        string? sessionId = null,
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
