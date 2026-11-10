namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// Host-level limits for model requests, including each MCP continuation.
/// 9.4 (sufi-chain#340): the default is no time limit — a model request ends only when the caller's
/// token is cancelled (user Stop, request aborted, circuit close, shutdown). Ops can still set
/// SufiAI:Transport:RequestTimeoutSeconds (1..3600) to reinstate a deadline if they want one.
/// </summary>
public class AITransportOptions
{
    public const string SectionName = "SufiAI:Transport";

    /// <summary>0 means no limit (the default). Values 1..3600 set an explicit deadline.</summary>
    public int RequestTimeoutSeconds { get; set; } = 0;

    /// <summary>
    /// Interactive chat (the Free Models Router included) must not sit on Thinking until a
    /// gateway eventually fails. When the host sets no transport deadline, chat still stops here.
    /// </summary>
    public const int DefaultChatRequestTimeoutSeconds = 20;

    public TimeSpan GetRequestTimeout()
    {
        if (RequestTimeoutSeconds == 0)
            return Timeout.InfiniteTimeSpan;
        if (RequestTimeoutSeconds is < 1 or > 3600)
            throw new ArgumentOutOfRangeException(nameof(RequestTimeoutSeconds),
                "AI request timeout must be 0 (no limit) or between 1 and 3600 seconds.");
        return TimeSpan.FromSeconds(RequestTimeoutSeconds);
    }

    /// <summary>
    /// Chat uses the host deadline when one is set, otherwise <see cref="DefaultChatRequestTimeoutSeconds"/>.
    /// Other model calls keep <see cref="GetRequestTimeout"/>.
    /// </summary>
    public TimeSpan GetChatRequestTimeout()
    {
        var configured = GetRequestTimeout();
        return configured == Timeout.InfiniteTimeSpan
            ? TimeSpan.FromSeconds(DefaultChatRequestTimeoutSeconds)
            : configured;
    }
}
