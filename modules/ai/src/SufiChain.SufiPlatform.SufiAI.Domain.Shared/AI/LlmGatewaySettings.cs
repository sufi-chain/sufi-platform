namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// Tenant-aware settings for the chat-completion gateway (sufi-chain#311).
/// Values are read through ABP settings, so a tenant override replaces the host default.
/// </summary>
public static class LlmGatewaySettings
{
    public const string Prefix = "SufiAI.LlmGateway.";

    public const string MaxAttempts = Prefix + "MaxAttempts";

    public const string BaseDelayMs = Prefix + "BaseDelayMs";

    public const string MaxDelayMs = Prefix + "MaxDelayMs";

    /// <summary>Percent of requests that may be retries. 10 means about 10 percent.</summary>
    public const string RetryBudgetPercent = Prefix + "RetryBudgetPercent";

    public const string BreakerErrorRatePercent = Prefix + "BreakerErrorRatePercent";

    public const string BreakerMinimumSamples = Prefix + "BreakerMinimumSamples";

    public const string BreakerWindow = Prefix + "BreakerWindow";

    public const string BreakerOpenSeconds = Prefix + "BreakerOpenSeconds";

    public const string MaxToolRounds = Prefix + "MaxToolRounds";

    /// <summary>Per-turn timeout in seconds. 0 keeps the turn open until the caller cancels it.</summary>
    public const string MaxTurnSeconds = Prefix + "MaxTurnSeconds";

    /// <summary>No built-in turn timer. A positive host or tenant value opts in.</summary>
    public const int DefaultMaxTurnSeconds = 0;
}

/// <summary>Finish reason for a stream that died after the client already received text.</summary>
public static class LlmGatewayFinishReasons
{
    public const string Interrupted = "interrupted";
}
