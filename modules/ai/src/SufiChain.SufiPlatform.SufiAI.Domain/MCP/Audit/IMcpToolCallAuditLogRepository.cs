using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace SufiChain.SufiPlatform.SufiAI.MCP.Audit;

/// <summary>
/// sufi-chain#384 slice 6a: read access to the tool-call audit trail. The tenant filter on the ambient
/// tenant applies as for every other repository, so a tenant never reads another tenant's rows.
/// </summary>
public interface IMcpToolCallAuditLogRepository : IRepository<McpToolCallAuditLog, Guid>
{
    /// <summary>Newest first, with optional filters.</summary>
    Task<List<McpToolCallAuditLog>> GetListAsync(
        string? toolName = null,
        Guid? userId = null,
        McpToolCallDecision? decision = null,
        int maxResultCount = 100,
        int skipCount = 0,
        CancellationToken cancellationToken = default);

    /// <summary>Row count for the same filters.</summary>
    Task<long> GetCountAsync(
        string? toolName = null,
        Guid? userId = null,
        McpToolCallDecision? decision = null,
        CancellationToken cancellationToken = default);
}
