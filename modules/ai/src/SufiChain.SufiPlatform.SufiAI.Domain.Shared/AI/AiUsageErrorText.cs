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
