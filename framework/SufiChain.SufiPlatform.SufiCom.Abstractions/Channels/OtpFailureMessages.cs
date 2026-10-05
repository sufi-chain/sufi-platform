using System.Globalization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace SufiChain.SufiPlatform.SufiCom.Channels;

/// <summary>
/// Localization keys for OTP gateway failures. Channels return <see cref="Format"/> text.
/// The application layer calls <see cref="Localize"/> before the text is stored or shown.
/// </summary>
public static class OtpFailureMessages
{
    public const string RequestFailed = "Communication:ProviderRequestFailed";

    public const string TimedOut = "Communication:ProviderRequestTimedOut";

    public const string InvalidPhone = "Communication:OtpInvalidPhone";

    public const string KavenegarRejected = "Communication:KavenegarOtpRejected";

    public const string SmsIrRejected = "Communication:SmsIrOtpRejected";

    public const string FanapRejected = "Communication:FanapOtpRejected";

    public static string Format(string key, params object[] args)
    {
        if (args.Length == 0)
        {
            return key;
        }

        var text = new string[args.Length];
        for (var i = 0; i < args.Length; i++)
        {
            text[i] = Convert.ToString(args[i], CultureInfo.InvariantCulture) ?? string.Empty;
        }

        return key + "|" + string.Join("|", text);
    }

    public static string Localize(IStringLocalizer localizer, string? coded)
    {
        if (string.IsNullOrWhiteSpace(coded))
        {
            return localizer[RequestFailed].Value;
        }

        var separator = coded.IndexOf('|');
        if (separator < 0)
        {
            return IsKey(coded) ? localizer[coded].Value : coded;
        }

        var key = coded.Substring(0, separator);
        if (!IsKey(key))
        {
            return coded;
        }

        var rawArgs = coded.Substring(separator + 1).Split('|');
        var args = new object[rawArgs.Length];
        for (var i = 0; i < rawArgs.Length; i++)
        {
            args[i] = rawArgs[i];
        }

        return localizer[key, args].Value;
    }

    /// <summary>
    /// Logs the exception type only. Provider exceptions can embed the request URL, and the Kavenegar URL contains the API key.
    /// </summary>
    public static void LogFailure(ILogger logger, string provider, Exception exception)
    {
        logger.LogError(
            "{Provider} OTP request failed. ExceptionType={ExceptionType}",
            provider,
            exception.GetType().Name);
    }

    public static void LogTimeout(ILogger logger, string provider)
    {
        logger.LogError("{Provider} OTP request timed out.", provider);
    }

    /// <summary>
    /// Logs the HTTP status and response body for a rejected gateway call.
    /// <paramref name="secrets"/> (API keys, tokens, OTP codes) are removed from the body first.
    /// </summary>
    public static void LogHttpFailure(
        ILogger logger,
        string provider,
        int httpStatus,
        int? providerStatus,
        string? body,
        params string?[] secrets)
    {
        logger.LogError(
            "{Provider} gateway request failed. HttpStatus={HttpStatus} ProviderStatus={ProviderStatus} Body={Body}",
            provider,
            httpStatus,
            providerStatus,
            Redact(body, secrets));
    }

    public static string Redact(string? body, params string?[] secrets)
    {
        if (string.IsNullOrEmpty(body))
        {
            return string.Empty;
        }

        var text = body;
        if (secrets != null)
        {
            foreach (var secret in secrets)
            {
                if (string.IsNullOrEmpty(secret) || secret.Length < 4)
                {
                    continue;
                }

                text = text.Replace(secret, "***", StringComparison.Ordinal);
            }
        }

        const int maxLength = 800;
        return text.Length <= maxLength ? text : text.Substring(0, maxLength);
    }

    private static bool IsKey(string value)
    {
        return value.StartsWith("Communication:", StringComparison.Ordinal)
            || value.StartsWith("Providers.", StringComparison.Ordinal)
            || value.StartsWith("SufiCom:", StringComparison.Ordinal);
    }
}
