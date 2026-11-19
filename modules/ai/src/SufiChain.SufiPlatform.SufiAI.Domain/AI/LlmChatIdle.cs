namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// Silence budget for one LLM HTTP call. A streaming reader calls <see cref="Arm"/>
/// after each received line so a slow model that keeps sending text is not cut off.
/// A non-streaming call has no intermediate bytes, so the budget is the wait for the body.
/// <see cref="Timeout.InfiniteTimeSpan"/> arms no timer.
/// </summary>
public sealed class LlmChatIdle : IDisposable
{
    private readonly CancellationTokenSource _source;
    private readonly TimeSpan _idle;

    public LlmChatIdle(CancellationToken caller, TimeSpan idle)
    {
        _source = CancellationTokenSource.CreateLinkedTokenSource(caller);
        _idle = idle;
        Arm();
    }

    public CancellationToken Token => _source.Token;

    public void Arm()
    {
        if (_idle > TimeSpan.Zero && _idle != Timeout.InfiniteTimeSpan)
        {
            _source.CancelAfter(_idle);
        }
    }

    public void Dispose() => _source.Dispose();
}
