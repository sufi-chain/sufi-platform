using System.Globalization;

namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// sufi-chain#551: the usage log stores <see cref="AIErrorCodes.ProviderRequestFailed"/> and the
/// screen localizes it without the status-code data, which left the raw <c>HTTP {StatusCode}</c> token.
/// That row shows <see cref="AIErrorCodes.ProviderRequestFailedBrief"/> instead.
/// </summary>
public static class AiUsageErrorText
{
    public static bool IsUnfilledProviderStatus(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        return string.Equals(text, AIErrorCodes.ProviderRequestFailed, StringComparison.Ordinal)
               || text.Contains("{StatusCode}", StringComparison.Ordinal);
    }
}

/// <summary>
/// Fills <c>{StatusCode}</c> / <c>{0}</c> in the provider-failure text. A missing code drops the
/// unfilled HTTP token so the modal never shows the raw placeholder.
/// </summary>
public static class AiProviderFailureMessage
{
    public static string Format(string? template, int? statusCode)
    {
        if (string.IsNullOrEmpty(template))
        {
            return template ?? string.Empty;
        }

        if (statusCode is not int code)
        {
            return template
                .Replace(" (HTTP {StatusCode})", string.Empty, StringComparison.Ordinal)
                .Replace("(HTTP {StatusCode})", string.Empty, StringComparison.Ordinal)
                .Replace("{StatusCode}", string.Empty, StringComparison.Ordinal);
        }

        var text = code.ToString(CultureInfo.InvariantCulture);
        return template
            .Replace("{StatusCode}", text, StringComparison.Ordinal)
            .Replace("{0}", text, StringComparison.Ordinal);
    }
}
