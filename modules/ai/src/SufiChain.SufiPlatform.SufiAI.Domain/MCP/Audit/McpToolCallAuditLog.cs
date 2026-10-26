using System;
using SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.SufiAI.MCP.Audit;

/// <summary>
/// sufi-chain#384 slice 6a: one append-only row per tool/MCP call, allowed or refused, bound to the
/// signed-in identity that the turn was bound to. Written through its own unit of work so a rolled-back
/// turn still leaves the audit trail. Mirrors <see cref="AIUsageLog"/>'s shape and tenancy.
/// </summary>
public class McpToolCallAuditLog : CreationAuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }

    /// <summary>The bound user the call ran for. Null means an anonymous turn.</summary>
    public Guid? UserId { get; protected set; }

    /// <summary>Conversation/session the call belongs to, when the turn knew it.</summary>
    public string? SessionId { get; protected set; }

    /// <summary>Hooshvare the turn came from, when the turn knew it.</summary>
    public Guid? HooshvareId { get; protected set; }

    /// <summary>Agent level in force for the call (null = the user's own permissions).</summary>
    public AgentToolScope? AgentScope { get; protected set; }

    public string ToolName { get; protected set; } = string.Empty;

    public MCPToolType ToolType { get; protected set; }

    /// <summary>True only when the tool is explicitly marked read-only (sufi-chain#384 slice 5 rule).</summary>
    public bool IsReadOnly { get; protected set; }

    /// <summary>Argument key names only; secret-like keys are redacted. Never carries argument values.</summary>
    public string? ArgumentsSummary { get; protected set; }

    public McpToolCallDecision Decision { get; protected set; }

    /// <summary>Refusal code or gate reason for a denied/needs-approval call; null for an allowed call.</summary>
    public string? DecisionReason { get; protected set; }

    public McpToolCallResultStatus ResultStatus { get; protected set; }

    public long DurationMs { get; protected set; }

    /// <summary>ABP correlation id of the turn, when the host provides one.</summary>
    public string? CorrelationId { get; protected set; }

    protected McpToolCallAuditLog()
    {
    }

    public McpToolCallAuditLog(Guid id) : base(id)
    {
    }

    public McpToolCallAuditLog Bind(AgentCallIdentity identity)
    {
        TenantId = identity.TenantId;
        UserId = identity.UserId;
        SessionId = identity.SessionId;
        HooshvareId = identity.HooshvareId;
        AgentScope = identity.AgentScope;
        return this;
    }

    public McpToolCallAuditLog ForTool(string toolName, MCPToolType toolType, bool isReadOnly)
    {
        ToolName = toolName;
        ToolType = toolType;
        IsReadOnly = isReadOnly;
        return this;
    }

    public McpToolCallAuditLog RecordArguments(string? argumentsSummary)
    {
        ArgumentsSummary = argumentsSummary;
        return this;
    }

    public McpToolCallAuditLog Decide(McpToolCallDecision decision, string? decisionReason = null)
    {
        Decision = decision;
        DecisionReason = string.IsNullOrWhiteSpace(decisionReason) ? null : Truncate(decisionReason!, DecisionReasonMaxLength);
        return this;
    }

    public McpToolCallAuditLog Complete(McpToolCallResultStatus status, long durationMs, string? correlationId)
    {
        ResultStatus = status;
        DurationMs = durationMs < 0 ? 0 : durationMs;
        CorrelationId = string.IsNullOrWhiteSpace(correlationId) ? null : Truncate(correlationId!, CorrelationIdMaxLength);
        return this;
    }

    public const int ToolNameMaxLength = 256;
    public const int SessionIdMaxLength = 128;
    public const int ArgumentsSummaryMaxLength = 1024;
    public const int DecisionReasonMaxLength = 512;
    public const int CorrelationIdMaxLength = 64;

    private static string Truncate(string value, int maxLength) =>
        value.Length <= maxLength ? value : value[..maxLength];
}
