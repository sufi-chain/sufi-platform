using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace SufiChain.SufiPlatform.SufiAI.MCP.Entities;

/// <summary>
/// Repository for MCPServer entities.
/// </summary>
public interface IMCPServerRepository : IRepository<MCPServer, Guid>
{
    /// <summary>
    /// Find server by immutable key within the current tenant.
    /// </summary>
    Task<MCPServer?> FindByKeyAsync(
        string key,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get all enabled servers for the current tenant.
    /// </summary>
    Task<List<MCPServer>> GetEnabledListAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get all servers for the current tenant.
    /// </summary>
    Task<List<MCPServer>> GetListAsync(CancellationToken cancellationToken = default);
}
