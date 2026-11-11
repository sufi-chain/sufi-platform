using System.Net.Http;
using System.Text.RegularExpressions;
using Volo.Abp;

namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// A provider failure the gateway can classify. Transport failures are transient.
/// 4xx, content-filter, and validation failures are not.
/// </summary>
public sealed class LlmProviderCallException : Exception
{
    public LlmProviderCallException(
        string message,
        int? statusCode = null,
        TimeSpan? retryAfter = null,
        string? errorCode = null,
        bool isTimeout = false,
        bool isConnectionReset = false,
        Exception? inner = null)
        : base(message, inner)
    {
        StatusCode = statusCode;
        RetryAfter = retryAfter;
        ErrorCode = errorCode;
        IsTimeout = isTimeout;
        IsConnectionReset = isConnectionReset;
    }

    public int? StatusCode { get; }

    public TimeSpan? RetryAfter { get; }

    public string? ErrorCode { get; }

    public bool IsTimeout { get; }

    public bool IsConnectionReset { get; }
}

public sealed class LlmCallFailure
{
    public Exception? Exception { get; init; }

    public int? StatusCode { get; init; }

    public TimeSpan? RetryAfter { get; init; }

    public string? ErrorCode { get; init; }

    public bool IsTransient { get; init; }

    public static LlmCallFailure Describe(Exception exception)
    {
        if (exception is LlmProviderCallException call)
        {
            return FromStatus(exception, call.StatusCode, call.RetryAfter, call.ErrorCode, call.IsTimeout || call.IsConnectionReset);
        }

        if (exception is BusinessException business)
        {
            int? status = null;
            TimeSpan? retryAfter = null;
            string? errorCode = null;
            if (business.Data.Contains("StatusCode") &&
                int.TryParse(business.Data["StatusCode"]?.ToString(), out var parsed))
            {
                status = parsed;
            }

            if (business.Data.Contains("RetryAfter") &&
                double.TryParse(business.Data["RetryAfter"]?.ToString(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var seconds))
            {
                retryAfter = TimeSpan.FromSeconds(seconds);
            }

            if (business.Data.Contains("ErrorCode"))
            {
                errorCode = business.Data["ErrorCode"]?.ToString();
            }

            var transient = business.Code != AIErrorCodes.GuardModelReply &&
                            IsTransientStatus(status, errorCode, forceTransient: false);
            if (business.Code == AIErrorCodes.GuardModelReply || IsContentFilter(errorCode) || IsValidation(business.Code))
            {
                transient = false;
            }

            return new LlmCallFailure
            {
                Exception = exception,
                StatusCode = status,
                RetryAfter = retryAfter,
                ErrorCode = errorCode,
                IsTransient = transient
            };
        }

        if (exception is TimeoutException)
        {
            return new LlmCallFailure { Exception = exception, IsTransient = true };
        }

        if (exception is HttpRequestException)
        {
            return new LlmCallFailure { Exception = exception, IsTransient = true };
        }

        return new LlmCallFailure { Exception = exception, IsTransient = false };
    }

    public static string DescribeUsageFailure(Exception exception)
    {
        var status = ReadStatus(exception);
        var reason = ReadProviderMessage(exception);
        if (!string.IsNullOrWhiteSpace(reason))
        {
            return status is int code ? $"HTTP {code}: {reason}" : reason;
        }

        if (exception is BusinessException business && !string.IsNullOrWhiteSpace(business.Code))
        {
            return status is int code ? $"{business.Code} (HTTP {code})" : business.Code;
        }

        var message = RedactSecrets(exception.Message);
        if (string.IsNullOrWhiteSpace(message)
            || LooksSensitive(message)
            || message.StartsWith("Exception of type '", StringComparison.Ordinal))
        {
            return status is int http ? $"HTTP {http}" : exception.GetType().Name;
        }

        return status is int withStatus ? $"HTTP {withStatus}: {message}" : message;
    }

    public static BusinessException Sanitize(Exception? exception)
    {
        var status = ReadStatus(exception);
        if (exception is BusinessException business && IsUserSafe(business))
        {
            if (ProviderFailureKind.IsGenericProviderFailure(business.Code))
            {
                var mapped = status is int ? ProviderFailureKind.CodeFor(status) : business.Code!;
                return CopyProviderFailure(business, mapped, status);
            }

            if (status is int existing && !HasStatus(business))
            {
                business.WithData("StatusCode", existing);
            }

            return business;
        }

        var safe = new BusinessException(ProviderFailureKind.CodeFor(status));
        if (status is int code)
        {
            safe.WithData("StatusCode", code);
        }

        var reason = ReadProviderMessage(exception);
        if (!string.IsNullOrWhiteSpace(reason))
        {
            safe.WithData("ProviderMessage", reason);
        }

        return safe;
    }

    /// <summary>
    /// Removes credential material before a provider body is written to a log.
    /// The usage row and the chat never receive the original text.
    /// </summary>
    public static string RedactSecrets(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        var text = SecretKey.Replace(value, "[redacted]");
        text = BearerToken.Replace(text, "Bearer [redacted]");
        return ApiKeyField.Replace(text, "$1[redacted]");
    }

    private static BusinessException CopyProviderFailure(BusinessException business, string code, int? status)
    {
        var copy = new BusinessException(code);
        if (status is int codeValue)
        {
            copy.WithData("StatusCode", codeValue);
        }

        var reason = ReadProviderMessage(business);
        if (!string.IsNullOrWhiteSpace(reason))
        {
            copy.WithData("ProviderMessage", reason);
        }

        return copy;
    }

    private static string? ReadProviderMessage(Exception? exception)
    {
        if (exception is not BusinessException business || !business.Data.Contains("ProviderMessage"))
        {
            return null;
        }

        var text = business.Data["ProviderMessage"]?.ToString();
        if (string.IsNullOrWhiteSpace(text) || LooksSensitive(text))
        {
            return null;
        }

        return text.Length <= 300 ? text : text[..300];
    }

    private static int? ReadStatus(Exception? exception)
    {
        if (exception is LlmProviderCallException call)
        {
            return call.StatusCode;
        }

        if (exception is BusinessException business &&
            business.Data.Contains("StatusCode") &&
            int.TryParse(business.Data["StatusCode"]?.ToString(), out var parsed))
        {
            return parsed;
        }

        return null;
    }

    private static bool HasStatus(BusinessException business)
    {
        return business.Data.Contains("StatusCode") && business.Data["StatusCode"] != null;
    }

    private static LlmCallFailure FromStatus(
        Exception exception,
        int? status,
        TimeSpan? retryAfter,
        string? errorCode,
        bool forceTransient)
    {
        return new LlmCallFailure
        {
            Exception = exception,
            StatusCode = status,
            RetryAfter = retryAfter,
            ErrorCode = errorCode,
            IsTransient = IsTransientStatus(status, errorCode, forceTransient)
        };
    }

    private static bool IsTransientStatus(int? status, string? errorCode, bool forceTransient)
    {
        if (IsContentFilter(errorCode))
        {
            return false;
        }

        if (forceTransient)
        {
            return true;
        }

        if (status is 429 or 408)
        {
            return true;
        }

        if (status >= 500)
        {
            return true;
        }

        if (status >= 400)
        {
            return false;
        }

        return false;
    }

    private static bool IsContentFilter(string? errorCode)
    {
        return string.Equals(errorCode, "content_filter", StringComparison.OrdinalIgnoreCase)
               || string.Equals(errorCode, "content_filter_violation", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsValidation(string? code)
    {
        return !string.IsNullOrWhiteSpace(code)
               && code.Contains("Validation", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsUserSafe(BusinessException business)
    {
        if (string.IsNullOrWhiteSpace(business.Code))
        {
            return false;
        }

        var text = business.Code + " " + business.Message;
        foreach (System.Collections.DictionaryEntry entry in business.Data)
        {
            text += " " + entry.Key + " " + entry.Value;
        }

        return !LooksSensitive(text);
    }

    public static bool LooksSensitive(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return false;
        }

        return text.Contains("http://", StringComparison.OrdinalIgnoreCase)
               || text.Contains("https://", StringComparison.OrdinalIgnoreCase)
               || text.Contains("sk-", StringComparison.OrdinalIgnoreCase)
               || text.Contains("api key", StringComparison.OrdinalIgnoreCase)
               || text.Contains("apikey", StringComparison.OrdinalIgnoreCase)
               || text.Contains("api_key", StringComparison.OrdinalIgnoreCase)
               || text.Contains("bearer ", StringComparison.OrdinalIgnoreCase)
               || text.Contains("stack trace", StringComparison.OrdinalIgnoreCase);
    }

    private static readonly Regex SecretKey = new(
        @"sk-[A-Za-z0-9_\-]{4,}",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex BearerToken = new(
        @"Bearer\s+\S+",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static readonly Regex ApiKeyField = new(
        @"((?:api[_-]?key|authorization)""?\s*[:=]\s*""?)([^""\s,}]+)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
}
