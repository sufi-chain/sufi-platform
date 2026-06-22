using System;
using System.Collections.Generic;

namespace SufiChain.SufiAbp.AI;

/// <summary>
/// Context for an AI tool execution within a workspace.
/// Tool implementations must respect the tenant/user context and never perform
/// cross-tenant lookups.
/// </summary>
public class SufiAIToolExecutionContext
{
    /// <summary>
    /// Workspace name the tool is executed for.
    /// </summary>
    public string WorkspaceName { get; set; } = string.Empty;

    /// <summary>
    /// Tenant identifier (<c>null</c> for host).
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// Identifier of the user on whose behalf the tool executes.
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Additional metadata for the execution.
    /// </summary>
    public Dictionary<string, object?>? Metadata { get; set; }
}
