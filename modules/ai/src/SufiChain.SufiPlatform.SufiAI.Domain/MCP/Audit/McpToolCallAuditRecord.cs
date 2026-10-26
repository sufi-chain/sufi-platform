using System;
using System.Collections.Generic;
using System.Linq;
using SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;

namespace SufiChain.SufiPlatform.SufiAI.MCP.Audit;

/// <summary>
/// One tool/MCP call to record. Carries only what an audit row needs; argument values are never kept.
/// </summary>
public sealed class McpToolCallAuditRecord
{
    public AgentCallIdentity Identity { get; init; } = new(null, null, null, null, null);

    public string ToolName { get; init; } = string.Empty;

    public MCPToolType ToolType { get; init; }

    public bool IsReadOnly { get; init; }

    /// <summary>Call arguments. Only the key names are summarised; secret-like keys are redacted.</summary>
    public IReadOnlyDictionary<string, object?>? Parameters { get; init; }

    public McpToolCallDecision Decision { get; init; }

    /// <summary>Refusal code or gate reason when the call was denied or held for approval.</summary>
    public string? DecisionReason { get; init; }

    public McpToolCallResultStatus ResultStatus { get; init; }

    public long DurationMs { get; init; }

    /// <summary>Builds a record straight off a turn's workspace context.</summary>
    public static McpToolCallAuditRecord Create(
        WorkspaceContext? context,
        string toolName,
        MCPToolType toolType,
        bool isReadOnly,
        IReadOnlyDictionary<string, object?>? parameters,
        McpToolCallDecision decision,
        string? decisionReason,
        McpToolCallResultStatus resultStatus,
        long durationMs)
    {
        return new McpToolCallAuditRecord
        {
            Identity = AgentCallIdentity.From(context),
            ToolName = toolName,
            ToolType = toolType,
            IsReadOnly = isReadOnly,
            Parameters = parameters,
            Decision = decision,
            DecisionReason = decisionReason,
            ResultStatus = resultStatus,
            DurationMs = durationMs
        };
    }
}

/// <summary>
/// sufi-chain#384 slice 6a: turns call arguments into a safe summary. Key names survive; a key that
/// looks secret keeps its name but never its value, and no argument value is ever stored at all.
/// </summary>
public static class McpToolCallAuditArguments
{
    private static readonly string[] SecretMarkers =
    {
        "password", "passwd", "secret", "token", "apikey", "api_key", "authorization",
        "credential", "bearer", "privatekey", "private_key", "clientsecret", "client_secret",
        "accesstoken", "access_token", "refreshtoken", "refresh_token", "connectionstring",
        "connstring", "signature", "otp", "pincode"
    };

    /// <summary>True when an argument key looks like it carries a credential or other sensitive value.</summary>
    public static bool IsSecretLike(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return false;
        }

        var normalized = key.Replace("-", string.Empty).Replace(" ", string.Empty).ToLowerInvariant();
        return SecretMarkers.Any(marker => normalized.Contains(marker, StringComparison.Ordinal));
    }

    /// <summary>
    /// Comma-separated key list, sorted, with secret-like keys suffixed <c>=redacted</c>. Null when there
    /// are no arguments, so an argument-less call stores no summary.
    /// </summary>
    public static string? Summarize(IReadOnlyDictionary<string, object?>? parameters)
    {
        if (parameters is null || parameters.Count == 0)
        {
            return null;
        }

        return string.Join(", ", parameters.Keys
            .Where(key => !string.IsNullOrWhiteSpace(key))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(key => key, StringComparer.Ordinal)
            .Select(key => IsSecretLike(key) ? key + "=redacted" : key));
    }
}
