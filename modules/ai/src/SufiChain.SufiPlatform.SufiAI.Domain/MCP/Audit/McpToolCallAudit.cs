using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;

namespace SufiChain.SufiPlatform.SufiAI.MCP.Audit;

/// <summary>
/// sufi-chain#384 slice 6a: shared plumbing so every entry path writes exactly one audit row per call.
/// The leaf tools (<see cref="MCPToolType.Internal"/>/<see cref="MCPToolType.External"/>) record the row
/// for a call that was actually dispatched; the pre-execution checks in the manager and the kernel
/// registrar record the row only when they refuse or hold the call, because the leaf is then never reached.
/// </summary>
public static class McpToolCallAudit
{
    /// <summary>
    /// Every refusal code that means the call was denied before the tool ran (sufi-chain#384 slices 1-5).
    /// A failure with any other message is a tool that ran and reported a problem.
    /// </summary>
    private static readonly HashSet<string> RefusalCodes = new(StringComparer.Ordinal)
    {
        AgentCallErrorCodes.CallContextMissing,
        AgentCallErrorCodes.PrincipalMismatch,
        AgentCallErrorCodes.TenantMismatch,
        AgentCallErrorCodes.ForgedContext,
        AgentCallErrorCodes.PermissionDenied,
        AgentCallErrorCodes.AnonymousNotAllowed,
        AgentCallErrorCodes.PermissionNotDeclared,
        AgentCallErrorCodes.IdentityServicesMissing,
        AgentCallErrorCodes.AgentScopeReadOnly
    };

    /// <summary>True when a failure message is a pre-execution refusal rather than a tool failure.</summary>
    public static bool IsRefusal(string? errorMessage) =>
        errorMessage is not null && RefusalCodes.Contains(errorMessage);

    /// <summary>Read-only interim rule (sufi-chain#384 slice 5): only an explicit <c>ReadOnly</c> tool counts.</summary>
    public static bool IsReadOnly(IMCPTool tool) => tool is IMCPToolEffect { IsReadOnly: true };

    /// <summary>Classifies a dispatched call's outcome.</summary>
    public static (McpToolCallDecision Decision, McpToolCallResultStatus Status, string? Reason) Classify(
        MCPToolExecutionResult result)
    {
        if (result.Success)
        {
            return (McpToolCallDecision.Allowed, McpToolCallResultStatus.Succeeded, null);
        }

        return IsRefusal(result.ErrorMessage)
            ? (McpToolCallDecision.Denied, McpToolCallResultStatus.Failed, result.ErrorMessage)
            : (McpToolCallDecision.Allowed, McpToolCallResultStatus.Failed, result.ErrorMessage);
    }

    /// <summary>Records a call that was refused or held before the tool ran. Never throws.</summary>
    public static Task RecordRefusedAsync(
        IMcpToolCallAuditor? auditor,
        IMCPTool tool,
        WorkspaceContext? context,
        IReadOnlyDictionary<string, object?>? parameters,
        McpToolCallDecision decision,
        string? reason,
        long durationMs = 0)
    {
        return RecordAsync(
            auditor,
            McpToolCallAuditRecord.Create(
                context,
                tool.Name,
                tool.ToolType,
                IsReadOnly(tool),
                parameters,
                decision,
                reason,
                McpToolCallResultStatus.Failed,
                durationMs));
    }

    /// <summary>Records a dispatched call from its result. Never throws.</summary>
    public static Task RecordDispatchedAsync(
        IMcpToolCallAuditor? auditor,
        IMCPTool tool,
        WorkspaceContext? context,
        IReadOnlyDictionary<string, object?>? parameters,
        MCPToolExecutionResult result,
        long durationMs)
    {
        var (decision, status, reason) = Classify(result);
        return RecordAsync(
            auditor,
            McpToolCallAuditRecord.Create(
                context,
                tool.Name,
                tool.ToolType,
                IsReadOnly(tool),
                parameters,
                decision,
                reason,
                status,
                durationMs));
    }

    /// <summary>Records a call that was cancelled mid-flight.</summary>
    public static Task RecordCancelledAsync(
        IMcpToolCallAuditor? auditor,
        IMCPTool tool,
        WorkspaceContext? context,
        IReadOnlyDictionary<string, object?>? parameters,
        long durationMs)
    {
        return RecordAsync(
            auditor,
            McpToolCallAuditRecord.Create(
                context,
                tool.Name,
                tool.ToolType,
                IsReadOnly(tool),
                parameters,
                McpToolCallDecision.Allowed,
                null,
                McpToolCallResultStatus.Cancelled,
                durationMs));
    }

    private static async Task RecordAsync(IMcpToolCallAuditor? auditor, McpToolCallAuditRecord record)
    {
        if (auditor is null)
        {
            // No auditor wired (unit tests, hosts without the AI EF layer): skip quietly.
            return;
        }

        try
        {
            await auditor.RecordAsync(record, CancellationToken.None);
        }
        catch (Exception)
        {
            // Belt and braces: the default auditor already swallows its own failures, but a custom one
            // must never be able to turn a finished tool call into a failed one.
        }
    }
}
