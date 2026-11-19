namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// Host-level limits for model requests, including each MCP continuation.
/// sufi-chain#551: chat completions have no hard-coded total cap. A model request ends when the
/// caller's token is cancelled (user Stop, request aborted, circuit close, shutdown), when
/// <see cref="RequestTimeoutSeconds"/> is set, or when a chat stream stays silent for
/// <see cref="ChatIdleTimeoutSeconds"/>.
/// </summary>
public class AITransportOptions
{
    public const string SectionName = "SufiAI:Transport";

    /// <summary>0 means no total limit (the default). Values 1..3600 set an explicit deadline.</summary>
    public int RequestTimeoutSeconds { get; set; }

    /// <summary>
    /// Silence budget for a chat completion, in seconds. 0 disables it.
    /// Streaming resets the budget on every received line. A non-streaming body has no
    /// intermediate bytes, so this is the wait for that body. The default is long enough
    /// that a reply slower than 60 seconds still completes.
    /// </summary>
    public const int DefaultChatIdleTimeoutSeconds = 300;

    public int ChatIdleTimeoutSeconds { get; set; } = DefaultChatIdleTimeoutSeconds;

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
    /// Chat uses the same total deadline as every other model call. There is no separate
    /// short chat cap. Silence is <see cref="GetChatIdleTimeout"/>.
    /// </summary>
    public TimeSpan GetChatRequestTimeout() => GetRequestTimeout();

    public TimeSpan GetChatIdleTimeout()
    {
        if (ChatIdleTimeoutSeconds == 0)
            return Timeout.InfiniteTimeSpan;
        if (ChatIdleTimeoutSeconds is < 1 or > 3600)
            throw new ArgumentOutOfRangeException(nameof(ChatIdleTimeoutSeconds),
                "Chat idle timeout must be 0 (no limit) or between 1 and 3600 seconds.");
        return TimeSpan.FromSeconds(ChatIdleTimeoutSeconds);
    }
}
