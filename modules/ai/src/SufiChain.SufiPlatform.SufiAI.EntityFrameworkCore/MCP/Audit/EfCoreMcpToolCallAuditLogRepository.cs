using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SufiChain.SufiPlatform.SufiAI.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiPlatform.SufiAI.MCP.Audit;

/// <summary>
/// EF Core repository for the tool-call audit trail. Reads are newest-first and honour the ambient
/// tenant filter, so a tenant only ever sees its own rows.
/// </summary>
public class EfCoreMcpToolCallAuditLogRepository
    : EfCoreRepository<IAIDbContext, McpToolCallAuditLog, Guid>,
      IMcpToolCallAuditLogRepository
{
    public EfCoreMcpToolCallAuditLogRepository(
        IDbContextProvider<IAIDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<List<McpToolCallAuditLog>> GetListAsync(
        string? toolName = null,
        Guid? userId = null,
        McpToolCallDecision? decision = null,
        int maxResultCount = 100,
        int skipCount = 0,
        CancellationToken cancellationToken = default)
    {
        var query = await ApplyFiltersAsync(toolName, userId, decision);

        return await query
            .OrderByDescending(log => log.CreationTime)
            .Skip(skipCount)
            .Take(maxResultCount)
            .ToListAsync(cancellationToken);
    }

    public async Task<long> GetCountAsync(
        string? toolName = null,
        Guid? userId = null,
        McpToolCallDecision? decision = null,
        CancellationToken cancellationToken = default)
    {
        var query = await ApplyFiltersAsync(toolName, userId, decision);
        return await query.LongCountAsync(cancellationToken);
    }

    private async Task<IQueryable<McpToolCallAuditLog>> ApplyFiltersAsync(
        string? toolName,
        Guid? userId,
        McpToolCallDecision? decision)
    {
        var dbSet = await GetDbSetAsync();
        var query = dbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(toolName))
        {
            var trimmed = toolName.Trim();
            query = query.Where(log => log.ToolName == trimmed);
        }

        if (userId is Guid user)
        {
            query = query.Where(log => log.UserId == user);
        }

        if (decision is McpToolCallDecision value)
        {
            query = query.Where(log => log.Decision == value);
        }

        return query;
    }
}
