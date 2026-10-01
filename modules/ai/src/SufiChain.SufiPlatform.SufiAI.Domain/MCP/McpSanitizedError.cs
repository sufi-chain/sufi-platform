using System.Text.RegularExpressions;

namespace SufiChain.SufiPlatform.SufiAI.MCP;

/// <summary>
/// Client-facing MCP failure. Includes a request id and a corrective action.
/// Stack traces, secrets, and authorization headers are removed.
/// </summary>
public sealed class McpSanitizedError
{
    private static readonly Regex SecretPattern = new(
        @"(?i)(bearer\s+)[^\s]+|(sk-[A-Za-z0-9_\-]{8,})|(api[_-]?key\s*[=:]\s*)\S+|(password\s*[=:]\s*)\S+|(pwd\s*[=:]\s*)\S+|(AccountKey\s*[=:]\s*)\S+",
        RegexOptions.Compiled);

    public string RequestId { get; }

    public string Summary { get; }

    public string CorrectiveAction { get; }

    public McpSanitizedError(string requestId, string summary, string correctiveAction)
    {
        RequestId = string.IsNullOrWhiteSpace(requestId)
            ? Guid.NewGuid().ToString("N")
            : requestId.Trim();
        Summary = string.IsNullOrWhiteSpace(summary) ? "The MCP tool call failed." : summary.Trim();
        CorrectiveAction = correctiveAction;
    }

    public static McpSanitizedError FromException(Exception exception, string? requestId)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return new McpSanitizedError(
            string.IsNullOrWhiteSpace(requestId) ? Guid.NewGuid().ToString("N") : requestId,
            Sanitize(exception.Message),
            "Confirm the MCP server is connected, the tool is on the hooshvare allowlist, and the workspace API mode is ChatCompletions. Retry without secrets in the tool arguments.");
    }

    public string ToClientMessage() =>
        $"MCP request {RequestId} failed. {Summary} Corrective action: {CorrectiveAction}";

    public static string Sanitize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "The MCP tool call failed.";
        }

        var lines = value
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Split('\n')
            .Where(line =>
                !line.TrimStart().StartsWith("at ", StringComparison.Ordinal)
                && !line.Contains("--- End of stack trace", StringComparison.Ordinal))
            .Select(line => SecretPattern.Replace(line, match =>
            {
                var text = match.Value;
                var separator = text.IndexOfAny([' ', '=', ':']);
                return separator < 0 ? "[redacted]" : text[..(separator + 1)] + "[redacted]";
            }));

        var sanitized = string.Join(' ', lines).Trim();
        return string.IsNullOrWhiteSpace(sanitized) ? "The MCP tool call failed." : sanitized;
    }
}
