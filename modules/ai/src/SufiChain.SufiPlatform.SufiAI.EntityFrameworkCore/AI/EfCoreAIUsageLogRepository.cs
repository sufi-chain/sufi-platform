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

public class EfCoreAIUsageLogRepository 
    : EfCoreRepository<IAIDbContext, AIUsageLog, Guid>, 
      IAIUsageLogRepository
{
    public EfCoreAIUsageLogRepository(
        IDbContextProvider<IAIDbContext> dbContextProvider) 
        : base(dbContextProvider)
    {
    }

    public async Task<List<AIUsageLog>> GetByWorkspaceAsync(
        Guid workspaceId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        var query = dbSet.Where(l => l.WorkspaceId == workspaceId);

        if (startDate.HasValue)
            query = query.Where(l => l.CreationTime >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(l => l.CreationTime <= endDate.Value);

        return await query
            .OrderByDescending(l => l.CreationTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<decimal> GetTotalCostAsync(
        Guid workspaceId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        var query = dbSet.Where(l => l.WorkspaceId == workspaceId && l.IsSuccess);

        if (startDate.HasValue)
            query = query.Where(l => l.CreationTime >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(l => l.CreationTime <= endDate.Value);

        return await query.SumAsync(l => l.EstimatedCost, cancellationToken);
    }

    public async Task<long> GetTotalTokensAsync(
        Guid workspaceId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        var query = dbSet.Where(l => l.WorkspaceId == workspaceId && l.IsSuccess);

        if (startDate.HasValue)
            query = query.Where(l => l.CreationTime >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(l => l.CreationTime <= endDate.Value);

        return await query.SumAsync(l => (long)(l.TotalTokens ?? 0), cancellationToken);
    }

    public async Task<List<AIUsageLog>> GetUnchargedSharedAsync(
        Guid tenantId,
        int maxResultCount,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        var query = dbSet.Where(l =>
                l.IsSharedWorkspaceUsage &&
                l.CreditChargedAt == null);
        return await query
            .OrderBy(l => l.CreationTime)
            .Take(maxResultCount)
            .ToListAsync(cancellationToken);
    }

    public async Task MarkChargedAsync(
        IEnumerable<AIUsageLog> logs,
        Guid batchId,
        DateTime chargedAt,
        CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        var ids = logs.Select(l => l.Id).ToList();
        var entries = await dbSet
            .Where(l => ids.Contains(l.Id))
            .ToListAsync(cancellationToken);
        foreach (var entry in entries)
        {
            entry.MarkCreditCharged(batchId, chargedAt);
        }
    }
}
