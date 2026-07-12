namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// Token usage information reported by an AI operation.
/// Consumers (e.g. product usage guards) should record usage once per completed request,
/// not per streaming chunk.
/// </summary>
public class SufiAITokenUsage
{
    /// <summary>
    /// Number of input (prompt) tokens, when reported by the provider.
    /// </summary>
    public int? InputTokens { get; set; }

    /// <summary>
    /// Number of output (completion) tokens, when reported by the provider.
    /// </summary>
    public int? OutputTokens { get; set; }

    /// <summary>
    /// Total tokens used, when reported by the provider.
    /// </summary>
    public int? TotalTokens { get; set; }

    /// <summary>
    /// Reason why usage information is unavailable, when the provider did not report it.
    /// </summary>
    public string? UnavailableReason { get; set; }

    /// <summary>
    /// Whether any token usage information is available.
    /// </summary>
    public bool HasUsage => InputTokens.HasValue || OutputTokens.HasValue || TotalTokens.HasValue;

    /// <summary>
    /// Resolves the total token count, falling back to input + output when the
    /// provider did not report a total.
    /// </summary>
    public int GetTotalOrSum()
    {
        return TotalTokens ?? (InputTokens ?? 0) + (OutputTokens ?? 0);
    }
}
