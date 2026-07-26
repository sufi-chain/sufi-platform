using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SufiChain.SufiPlatform.SufiAI.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiPlatform.SufiAI;

public class EfCoreAIModelConfigurationRepository 
    : EfCoreRepository<IAIDbContext, AIModelConfiguration, Guid>, 
      IAIModelConfigurationRepository
{
    public EfCoreAIModelConfigurationRepository(
        IDbContextProvider<IAIDbContext> dbContextProvider) 
        : base(dbContextProvider)
    {
    }

    public async Task<List<AIModelConfiguration>> GetByWorkspaceIdAsync(
        Guid workspaceId,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Where(c => c.WorkspaceId == workspaceId)
            .OrderBy(c => c.CapabilityType)
            .ThenBy(c => c.Priority)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<AIModelConfiguration>> GetEnabledByCapabilityAsync(
        Guid workspaceId,
        AICapabilityType capabilityType,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Where(c => c.WorkspaceId == workspaceId 
                     && c.CapabilityType == capabilityType 
                     && c.IsEnabled)
            .OrderBy(c => c.Priority)
            .ToListAsync(cancellationToken);
    }

    public async Task<AIModelConfiguration?> GetPrimaryConfigurationAsync(
        Guid workspaceId,
        AICapabilityType capabilityType,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Where(c => c.WorkspaceId == workspaceId 
                     && c.CapabilityType == capabilityType 
                     && c.IsEnabled)
            .OrderBy(c => c.Priority)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
