using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SufiChain.SufiAbp.AIManagement.MCP.Abstractions;

/// <summary>
/// Represents an MCP tool that can be executed by AI models.
/// Tools can be internal (ApplicationService methods) or external (MCP servers).
/// </summary>
public interface IMCPTool
{
    /// <summary>
    /// Unique identifier for the tool.
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// Human-readable description of what the tool does.
    /// </summary>
    string Description { get; }
    
    /// <summary>
    /// JSON schema describing the tool's parameters.
    /// </summary>
    string ParameterSchema { get; }
    
    /// <summary>
    /// Indicates whether this is an internal tool (ApplicationService method)
    /// or external tool (MCP server).
    /// </summary>
    MCPToolType ToolType { get; }
    
    /// <summary>
    /// Source identifier (e.g., service name for internal, server ID for external).
    /// </summary>
    string Source { get; }
    
    /// <summary>
    /// Execute the tool with the given parameters in a workspace context.
    /// </summary>
    Task<MCPToolExecutionResult> ExecuteAsync(
        WorkspaceContext context,
        Dictionary<string, object?> parameters,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Type of MCP tool.
/// </summary>
public enum MCPToolType
{
    /// <summary>
    /// Internal ApplicationService method marked with [MCPTool].
    /// </summary>
    Internal,
    
    /// <summary>
    /// External MCP server tool.
    /// </summary>
    External
}
