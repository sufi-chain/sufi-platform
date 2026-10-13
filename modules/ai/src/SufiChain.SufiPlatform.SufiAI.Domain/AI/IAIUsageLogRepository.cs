using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace SufiChain.SufiPlatform.SufiAI;

public interface IAIUsageLogRepository : IRepository<AIUsageLog, Guid>
{
    /// <summary>
    /// Get usage logs for a workspace within a date range
    /// </summary>
    Task<List<AIUsageLog>> GetByWorkspaceAsync(
        Guid workspaceId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get total cost for a workspace within a date range
    /// </summary>
    Task<decimal> GetTotalCostAsync(
        Guid workspaceId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get total tokens used for a workspace within a date range
    /// </summary>
    Task<long> GetTotalTokensAsync(
        Guid workspaceId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Uncharged shared-workspace usage logs for a tenant, ordered by creation time.
    /// Used by the AI Credit batch charger (A7).
    /// </summary>
    Task<List<AIUsageLog>> GetUnchargedSharedAsync(
        Guid tenantId,
        int maxResultCount,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Uncharged successful shared logs for any tenant (host worker batch).
    /// </summary>
    Task<List<AIUsageLog>> GetUnchargedSharedAllAsync(
        int maxResultCount,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Mark a batch of logs as charged at the given time.
    /// </summary>
    Task MarkChargedAsync(
        IEnumerable<AIUsageLog> logs,
        Guid batchId,
        DateTime chargedAt,
        CancellationToken cancellationToken = default);
}
