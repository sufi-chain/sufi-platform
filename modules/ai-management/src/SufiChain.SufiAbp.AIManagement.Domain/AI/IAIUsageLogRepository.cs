using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace SufiChain.SufiAbp.AIManagement.AI;

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
}
