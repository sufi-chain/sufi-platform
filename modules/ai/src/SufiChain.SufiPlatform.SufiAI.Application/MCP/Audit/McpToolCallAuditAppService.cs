using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SufiChain.SufiPlatform.Application.Dtos;
using SufiChain.SufiPlatform.Application.Services;
using SufiChain.SufiPlatform.Features;
using SufiChain.SufiPlatform.SufiAI.Features;
using SufiChain.SufiPlatform.SufiAI.Permissions;

namespace SufiChain.SufiPlatform.SufiAI.MCP.Audit;

/// <summary>
/// sufi-chain#384 slice 6a: read-only list of tool/MCP calls behind <c>SufiAI.McpAudit</c>.
/// Result rows are tenant-filtered by the ambient tenant through the repository's multi-tenant filter,
/// exactly as the workspace list is.
/// </summary>
[RequiresFeature(SufiAIFeatures.Enable)]
[Authorize(AIPermissions.McpAudit.Default)]
public class McpToolCallAuditAppService : SufiApplicationService, IMcpToolCallAuditAppService
{
    private readonly IMcpToolCallAuditLogRepository _repository;

    public McpToolCallAuditAppService(IMcpToolCallAuditLogRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResultDto<McpToolCallAuditLogDto>> GetListAsync(GetMcpToolCallAuditLogsInput input)
    {
        var decision = ParseDecision(input.Decision);
        var maxResultCount = input.MaxResultCount <= 0 ? 100 : input.MaxResultCount;
        var skipCount = input.SkipCount < 0 ? 0 : input.SkipCount;

        var items = await _repository.GetListAsync(
            input.ToolName,
            input.UserId,
            decision,
            maxResultCount,
            skipCount);

        var totalCount = await _repository.GetCountAsync(input.ToolName, input.UserId, decision);

        return new PagedResultDto<McpToolCallAuditLogDto>(
            totalCount,
            items.Select(Map).ToList());
    }

    private static McpToolCallDecision? ParseDecision(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return Enum.TryParse<McpToolCallDecision>(value.Trim(), ignoreCase: true, out var parsed)
            ? parsed
            : null;
    }

    private static McpToolCallAuditLogDto Map(McpToolCallAuditLog log)
    {
        return new McpToolCallAuditLogDto
        {
            Id = log.Id,
            TenantId = log.TenantId,
            UserId = log.UserId,
            SessionId = log.SessionId,
            HooshvareId = log.HooshvareId,
            AgentScope = log.AgentScope?.ToString(),
            ToolName = log.ToolName,
            ToolType = log.ToolType.ToString(),
            IsReadOnly = log.IsReadOnly,
            ArgumentsSummary = log.ArgumentsSummary,
            Decision = log.Decision.ToString(),
            DecisionReason = log.DecisionReason,
            ResultStatus = log.ResultStatus.ToString(),
            DurationMs = log.DurationMs,
            CorrelationId = log.CorrelationId,
            CreationTime = log.CreationTime
        };
    }
}
