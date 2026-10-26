using System;
using System.ComponentModel.DataAnnotations;
using SufiChain.SufiPlatform.Application.Dtos;

namespace SufiChain.SufiPlatform.SufiAI.MCP.Audit;

/// <summary>
/// sufi-chain#384 slice 6a: one tool/MCP call in the audit trail, as read back by an admin.
/// </summary>
public class McpToolCallAuditLogDto : EntityDto<Guid>
{
    public Guid? TenantId { get; set; }

    /// <summary>Null for an anonymous turn.</summary>
    public Guid? UserId { get; set; }

    public string? SessionId { get; set; }
    public Guid? HooshvareId { get; set; }
    public string? AgentScope { get; set; }
    public string ToolName { get; set; } = string.Empty;
    public string ToolType { get; set; } = string.Empty;
    public bool IsReadOnly { get; set; }
    public string? ArgumentsSummary { get; set; }
    public string Decision { get; set; } = string.Empty;
    public string? DecisionReason { get; set; }
    public string ResultStatus { get; set; } = string.Empty;
    public long DurationMs { get; set; }
    public string? CorrelationId { get; set; }
    public DateTime CreationTime { get; set; }
}

/// <summary>Filters for the audit trail list.</summary>
public class GetMcpToolCallAuditLogsInput : PagedAndSortedResultRequestDto
{
    /// <summary>Exact or prefix match on the tool name.</summary>
    [StringLength(256)]
    public string? ToolName { get; set; }

    public Guid? UserId { get; set; }

    /// <summary>One of <c>Allowed</c>, <c>Denied</c>, <c>NeedsApproval</c>.</summary>
    [StringLength(32)]
    public string? Decision { get; set; }
}
