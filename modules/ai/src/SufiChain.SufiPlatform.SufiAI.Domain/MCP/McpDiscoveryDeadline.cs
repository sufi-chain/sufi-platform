namespace SufiChain.SufiPlatform.SufiAI.MCP;

/// <summary>
/// sufi-chain#557: discovery ends when the caller cancels, or when
/// <see cref="AITransportOptions"/> sets a deadline. There is no separate hardcoded timeout.
/// <see cref="Timeout.InfiniteTimeSpan"/> (the default, <c>RequestTimeoutSeconds = 0</c>) adds no deadline.
/// </summary>
internal static class McpDiscoveryDeadline
{
    public static CancellationTokenSource Create(CancellationToken cancellationToken, TimeSpan requestTimeout)
    {
        var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        if (requestTimeout > TimeSpan.Zero && requestTimeout != Timeout.InfiniteTimeSpan)
        {
            linked.CancelAfter(requestTimeout);
        }

        return linked;
    }
}
