namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>Host-level limits for model requests, including each MCP continuation.</summary>
public class AITransportOptions
{
    public const string SectionName = "SufiAI:Transport";

    public int RequestTimeoutSeconds { get; set; } = 900;

    public TimeSpan GetRequestTimeout()
    {
        if (RequestTimeoutSeconds is < 1 or > 3600)
            throw new ArgumentOutOfRangeException(nameof(RequestTimeoutSeconds),
                "AI request timeout must be between 1 and 3600 seconds.");
        return TimeSpan.FromSeconds(RequestTimeoutSeconds);
    }
}
