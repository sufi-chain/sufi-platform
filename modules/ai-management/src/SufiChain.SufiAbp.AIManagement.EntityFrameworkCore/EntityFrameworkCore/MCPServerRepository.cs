using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SufiChain.SufiAbp.AIManagement.MCP.Entities;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiAbp.AIManagement.EntityFrameworkCore;

public class MCPServerRepository : EfCoreRepository<AIManagementDbContext, MCPServer, Guid>, IMCPServerRepository
{
    public MCPServerRepository(IDbContextProvider<AIManagementDbContext> dbContextProvider)
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
