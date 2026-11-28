namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// Shared 70/85/95 conversation budget for admin workspace chat and Hooshvare chats.
/// </summary>
public static class ContextBudget
{
    public const int WarningPercent = 70;
    public const int SummarizePercent = 85;
    public const int HardStopPercent = 95;

    public static string State(decimal percent, int warning = WarningPercent, int summarize = SummarizePercent, int hardStop = HardStopPercent)
    {
        if (percent >= hardStop)
        {
            return "HardStop";
        }

        if (percent >= summarize)
        {
            return "Summarize";
        }

        if (percent >= warning)
        {
            return "Warning";
        }

        return "Normal";
    }

    public static bool BlocksSend(string? state) =>
        string.Equals(state, "HardStop", StringComparison.Ordinal);

    public static bool OffersStartNewOrSummarize(string? state) =>
        string.Equals(state, "HardStop", StringComparison.Ordinal)
        || string.Equals(state, "Summarize", StringComparison.Ordinal);

    public static decimal Percent(int usedTokens, int maxTokens)
    {
        var max = maxTokens <= 0 ? 1 : maxTokens;
        return Math.Round(usedTokens * 100m / max, 2);
    }

    public static async Task<string> SummarizeOrAccountingAsync(
        Func<Task<string>> model,
        string accounting,
        Action<Exception>? log)
    {
        try
        {
            var text = (await model()).Trim();
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new InvalidOperationException("The model returned an empty summary.");
            }

            return text;
        }
        catch (Exception exception)
        {
            log?.Invoke(exception);
            return accounting;
        }
    }
}
