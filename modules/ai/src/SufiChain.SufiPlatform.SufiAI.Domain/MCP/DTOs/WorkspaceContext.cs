using System;
using System.Collections.Generic;

namespace SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;

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

    /// <summary>
    /// Verified identity the call runs for (sufi-chain#384). Internal tools refuse to run without it,
    /// and run under it rather than the ambient principal.
    /// </summary>
    public AgentCallContext? CallContext { get; set; }
}
