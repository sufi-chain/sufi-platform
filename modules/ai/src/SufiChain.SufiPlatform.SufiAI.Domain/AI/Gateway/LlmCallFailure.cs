using System.Net.Http;
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

    public static BusinessException Sanitize(Exception? exception)
    {
        if (exception is BusinessException business && IsUserSafe(business))
        {
            return business;
        }

        var safe = new BusinessException(AIErrorCodes.ProviderRequestFailed);
        if (exception is LlmProviderCallException call && call.StatusCode is int status)
        {
            safe.WithData("StatusCode", status);
        }

        return safe;
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
               || text.Contains("stack trace", StringComparison.OrdinalIgnoreCase);
    }
}
