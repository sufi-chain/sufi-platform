using SufiChain.SufiPlatform.SufiAI.MCP;
using Volo.Abp;

namespace SufiChain.SufiPlatform.SufiAI.MCP.Registry;

/// <summary>
/// sufi-chain#557: the text stored on the server and shown in the MCP servers grid.
/// A <see cref="BusinessException"/> constructed with only an error code has the message
/// "Exception of type 'Volo.Abp.BusinessException' was thrown." The cause lives in
/// <c>Data["Error"]</c>, <c>Data["Reason"]</c>, or <see cref="Exception.InnerException"/>.
/// </summary>
internal static class McpConnectionError
{
    public const int MaxLength = 1024;

    public static string Describe(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        string? code = null;
        for (var current = exception; current != null; current = current.InnerException)
        {
            if (current is BusinessException business)
            {
                if (string.IsNullOrWhiteSpace(code))
                {
                    code = business.Code;
                }

                if (TryData(business, "Error", out var error))
                {
                    return Limit(error);
                }

                if (TryData(business, "Reason", out var reason))
                {
                    return Limit(reason);
                }
            }

            if (IsUseful(current.Message))
            {
                return Limit(current.Message);
            }
        }

        return Limit(string.IsNullOrWhiteSpace(code) ? "Connection failed." : code);
    }

    private static bool TryData(Exception exception, string key, out string value)
    {
        value = string.Empty;
        if (!exception.Data.Contains(key) || exception.Data[key] is not string text || !IsUseful(text))
        {
            return false;
        }

        value = text;
        return true;
    }

    private static bool IsUseful(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return false;
        }

        return !message.StartsWith("Exception of type ", StringComparison.Ordinal)
            || !message.EndsWith(" was thrown.", StringComparison.Ordinal);
    }

    private static string Limit(string message)
    {
        var sanitized = McpSanitizedError.Sanitize(message);
        return sanitized.Length <= MaxLength ? sanitized : sanitized[..MaxLength];
    }
}
