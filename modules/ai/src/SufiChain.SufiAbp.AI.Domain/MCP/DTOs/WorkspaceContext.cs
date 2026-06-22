using System;

namespace SufiChain.SufiAbp.AI.MCP.Abstractions;

/// <summary>
/// Context information for tool execution within a workspace.
/// </summary>
public class WorkspaceContext
{
    /// <summary>
    /// Workspace name.
    /// </summary>
    public string WorkspaceName { get; set; } = string.Empty;
    
    /// <summary>
    /// Tenant ID (null for host).
    /// </summary>
    public Guid? TenantId { get; set; }
    
    /// <summary>
    /// User ID executing the tool.
    /// </summary>
    public Guid? UserId { get; set; }
    
    /// <summary>
    /// Additional metadata for tool execution.
    /// </summary>
    public Dictionary<string, object?>? Metadata { get; set; }
}
