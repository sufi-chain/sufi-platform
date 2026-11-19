using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SufiChain.SufiPlatform.SufiAI.EntityFrameworkCore;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiPlatform.SufiAI;

public class EfCoreAIModelConfigurationRepository 
    : EfCoreRepository<IAIDbContext, AIModelConfiguration, Guid>, 
      IAIModelConfigurationRepository
{
    private readonly IWorkspaceRepository _workspaceRepository;

    public EfCoreAIModelConfigurationRepository(
        IDbContextProvider<IAIDbContext> dbContextProvider,
        IWorkspaceRepository workspaceRepository) 
        : base(dbContextProvider)
    {
        _workspaceRepository = workspaceRepository;
    }

    public async Task<List<AIModelConfiguration>> GetByWorkspaceIdAsync(
        Guid workspaceId,
        CancellationToken cancellationToken = default)
    {
        var live = await GetInheritedRoutesAsync(workspaceId, cancellationToken);
        if (live != null)
        {
            return live
                .OrderBy(c => c.CapabilityType)
                .ThenBy(c => c.Priority)
                .ThenBy(c => c.CreationTime)
                .ThenBy(c => c.Id)
                .ToList();
        }

        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Where(c => c.WorkspaceId == workspaceId)
            .OrderBy(c => c.CapabilityType)
            .ThenBy(c => c.Priority)
            .ThenBy(c => c.CreationTime)
            .ThenBy(c => c.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<AIModelConfiguration>> GetEnabledByCapabilityAsync(
        Guid workspaceId,
        AICapabilityType capabilityType,
        CancellationToken cancellationToken = default)
    {
        var live = await GetInheritedRoutesAsync(workspaceId, cancellationToken);
        if (live != null)
        {
            return live
                .Where(c => c.CapabilityType == capabilityType && c.IsEnabled)
                .OrderBy(c => c.Priority)
                .ThenBy(c => c.CreationTime)
                .ThenBy(c => c.Id)
                .ToList();
        }

        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Where(c => c.WorkspaceId == workspaceId 
                     && c.CapabilityType == capabilityType 
                     && c.IsEnabled)
            .OrderBy(c => c.Priority)
            .ThenBy(c => c.CreationTime)
            .ThenBy(c => c.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<AIModelConfiguration?> GetPrimaryConfigurationAsync(
        Guid workspaceId,
        AICapabilityType capabilityType,
        CancellationToken cancellationToken = default)
    {
        var enabled = await GetEnabledByCapabilityAsync(workspaceId, capabilityType, cancellationToken);
        return enabled.FirstOrDefault();
    }

    /// <summary>
    /// Live host routes for an inherited projection. Null when the workspace is not inherited.
    /// </summary>
    private async Task<IReadOnlyList<AIModelConfiguration>?> GetInheritedRoutesAsync(
        Guid workspaceId,
        CancellationToken cancellationToken)
    {
        var workspace = await _workspaceRepository.FindAsync(
            workspaceId,
            includeDetails: true,
            cancellationToken: cancellationToken);
        if (workspace is not { IsInherited: true })
        {
            return null;
        }

        return workspace.ModelConfigurations;
    }
}
