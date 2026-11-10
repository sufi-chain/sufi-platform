using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SufiChain.SufiPlatform.SufiAI.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.MultiTenancy;

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
            l.IsSuccess &&
            l.CreditChargedAt == null &&
            l.TenantId == tenantId);
        return await query
            .OrderBy(l => l.CreationTime)
            .Take(maxResultCount)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Host worker entry: all uncharged successful shared logs (any tenant), oldest first.
    /// The host current-tenant filter would hide every tenant row, so the filter is off for this query.
    /// A tenant with its own database is not in this result; the charge worker switches tenant and queries again.
    /// </summary>
    public async Task<List<AIUsageLog>> GetUnchargedSharedAllAsync(
        int maxResultCount,
        CancellationToken cancellationToken = default)
    {
        using (DataFilter.Disable<IMultiTenant>())
        {
            var dbSet = await GetDbSetAsync();
            return await dbSet
                .Where(l =>
                    l.IsSharedWorkspaceUsage &&
                    l.IsSuccess &&
                    l.CreditChargedAt == null &&
                    l.TenantId != null)
                .OrderBy(l => l.CreationTime)
                .Take(maxResultCount)
                .ToListAsync(cancellationToken);
        }
    }

    public async Task MarkChargedAsync(
        IEnumerable<AIUsageLog> logs,
        Guid batchId,
        DateTime chargedAt,
        CancellationToken cancellationToken = default)
    {
        // The host worker marks tenant rows. The tenant filter would otherwise match nothing.
        using (DataFilter.Disable<IMultiTenant>())
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
}
