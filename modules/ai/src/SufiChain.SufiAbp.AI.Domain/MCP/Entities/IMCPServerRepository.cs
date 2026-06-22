using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace SufiChain.SufiAbp.AI.MCP.Entities;

/// <summary>
/// Repository for MCPServer entities.
/// </summary>
public interface IMCPServerRepository : IRepository<MCPServer, Guid>
{
    /// <summary>
    /// Find server by name within a workspace.
    /// </summary>
    Task<MCPServer?> FindByNameAsync(
        Guid workspaceId,
        string name,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get all enabled servers for a workspace.
    /// </summary>
    Task<List<MCPServer>> GetEnabledByWorkspaceAsync(
        Guid workspaceId,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get all servers for a workspace (enabled and disabled).
    /// </summary>
    Task<List<MCPServer>> GetByWorkspaceAsync(
        Guid workspaceId,
        CancellationToken cancellationToken = default);
}
