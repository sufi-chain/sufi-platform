namespace SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;

/// <summary>
/// Flows the current <see cref="AgentCallContext"/> across the in-process tool invocation.
/// <see cref="AgentCallContext"/> itself is unchanged: this only publishes the context the tool
/// dispatcher already built so application code can tell a human request from an agent turn.
/// The value is cleared when the tool returns, including after an exception.
/// </summary>
public static class AgentCallAmbient
{
    private static readonly AsyncLocal<AgentCallContext?> CurrentCall = new();

    /// <summary>The agent call whose tool is running, or null for a human request.</summary>
    public static AgentCallContext? Current => CurrentCall.Value;

    public static IDisposable Begin(AgentCallContext call)
    {
        ArgumentNullException.ThrowIfNull(call);
        var previous = CurrentCall.Value;
        CurrentCall.Value = call;
        return new Restore(previous);
    }

    private sealed class Restore : IDisposable
    {
        private readonly AgentCallContext? _previous;
        private bool _disposed;

        public Restore(AgentCallContext? previous)
        {
            _previous = previous;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            CurrentCall.Value = _previous;
        }
    }
}
