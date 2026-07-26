using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SufiChain.SufiPlatform.SufiAI.MCP.Entities;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiPlatform.SufiAI.EntityFrameworkCore;

public class MCPServerRepository : EfCoreRepository<IAIDbContext, MCPServer, Guid>, IMCPServerRepository
{
    public MCPServerRepository(IDbContextProvider<IAIDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }
    
    public async Task<MCPServer?> FindByKeyAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Where(s => s.Key == key)
            .FirstOrDefaultAsync(cancellationToken);
    }
    
    public async Task<List<MCPServer>> GetEnabledListAsync(CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Where(s => s.IsEnabled)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<List<MCPServer>> GetListAsync(CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .OrderBy(s => s.Key)
            .ToListAsync(cancellationToken);
    }
}
