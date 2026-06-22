using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SufiChain.SufiAbp.AI.MCP.Entities;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiAbp.AI.EntityFrameworkCore;

public class MCPServerRepository : EfCoreRepository<AIDbContext, MCPServer, Guid>, IMCPServerRepository
{
    public MCPServerRepository(IDbContextProvider<AIDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }
    
    public async Task<MCPServer?> FindByNameAsync(
        Guid workspaceId,
        string name,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Where(s => s.WorkspaceId == workspaceId && s.Name == name)
            .FirstOrDefaultAsync(cancellationToken);
    }
    
    public async Task<List<MCPServer>> GetEnabledByWorkspaceAsync(
        Guid workspaceId,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Where(s => s.WorkspaceId == workspaceId && s.IsEnabled)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<List<MCPServer>> GetByWorkspaceAsync(
        Guid workspaceId,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Where(s => s.WorkspaceId == workspaceId)
            .ToListAsync(cancellationToken);
    }
}
