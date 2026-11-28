using System.Text.RegularExpressions;

namespace SufiChain.SufiPlatform.SufiAI.MCP;

/// <summary>
/// Client-facing MCP failure. Includes a request id and a corrective action.
/// Stack traces, secrets, and authorization headers are removed.
/// </summary>
public sealed class McpSanitizedError
{
    public const int ConnectionSummaryMaxLength = 200;

    private static readonly Regex SecretPattern = new(
        @"(?i)(bearer\s+)[^\s]+|(sk-[A-Za-z0-9_\-]{8,})|(api[_-]?key\s*[=:]\s*)\S+|(password\s*[=:]\s*)\S+|(pwd\s*[=:]\s*)\S+|(AccountKey\s*[=:]\s*)\S+|(authorization\s*[:=]\s*)\S+|(x-api-key\s*[:=]\s*)\S+",
        RegexOptions.Compiled);

    private static readonly Regex HtmlTags = new(@"<[^>]*>", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex UrlWithSecrets = new(
        @"\b(?<scheme>[a-z][a-z0-9+.-]*)://(?:[^@/\s]+@)?(?<host>[^/?#\s]+)[^\s]*",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex HttpStatus = new(
        @"HTTP\s+(?<code>\d{3})\s*(?<reason>[A-Za-z][A-Za-z ]{0,40})?",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

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
            .Select(line => HtmlTags.Replace(line, " "))
            .Select(line => UrlWithSecrets.Replace(line, "${scheme}://${host}"))
            .Select(line => SecretPattern.Replace(line, match =>
            {
                var text = match.Value;
                var separator = text.IndexOfAny([' ', '=', ':']);
                return separator < 0 ? "[redacted]" : text[..(separator + 1)] + "[redacted]";
            }));

        var sanitized = string.Join(' ', lines);
        sanitized = Regex.Replace(sanitized, @"\s+", " ").Trim();
        return string.IsNullOrWhiteSpace(sanitized) ? "The MCP tool call failed." : sanitized;
    }

    public static string SummarizeConnection(string? value)
    {
        var text = Classify(Sanitize(value));
        return text.Length <= ConnectionSummaryMaxLength
            ? text
            : text[..ConnectionSummaryMaxLength];
    }

    private static string Classify(string text)
    {
        if (text.Contains("timed out", StringComparison.OrdinalIgnoreCase)
            || text.Contains("timeout", StringComparison.OrdinalIgnoreCase))
        {
            return "The connection timed out.";
        }

        if (text.Contains("connection refused", StringComparison.OrdinalIgnoreCase)
            || text.Contains("actively refused", StringComparison.OrdinalIgnoreCase))
        {
            return "The connection was refused.";
        }

        if (text.Contains("no such host", StringComparison.OrdinalIgnoreCase)
            || text.Contains("name or service not known", StringComparison.OrdinalIgnoreCase))
        {
            return "The host name could not be resolved.";
        }

        if (text.Contains("ssl", StringComparison.OrdinalIgnoreCase)
            || text.Contains("certificate", StringComparison.OrdinalIgnoreCase))
        {
            return "The TLS handshake failed.";
        }

        var status = HttpStatus.Match(text);
        if (!status.Success)
        {
            return text;
        }

        var code = status.Groups["code"].Value;
        var reason = status.Groups["reason"].Value.Trim();
        if (code is "401" or "403")
        {
            return string.IsNullOrEmpty(reason)
                ? $"HTTP {code}: authentication was rejected."
                : $"HTTP {code} {reason}: authentication was rejected.";
        }

        return string.IsNullOrEmpty(reason)
            ? $"HTTP {code}: the URL did not answer as an MCP server."
            : $"HTTP {code} {reason}: the URL did not answer as an MCP server.";
    }
}
