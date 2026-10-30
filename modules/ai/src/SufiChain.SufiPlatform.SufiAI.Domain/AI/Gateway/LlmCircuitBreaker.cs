namespace SufiChain.SufiPlatform.SufiAI;

public enum LlmCircuitState
{
    Closed = 0,
    Open = 1,
    HalfOpen = 2
}

public sealed class LlmCircuitSnapshot
{
    public LlmCircuitState State { get; set; } = LlmCircuitState.Closed;

    public List<bool> Failures { get; set; } = new();

    public DateTimeOffset? OpenedAt { get; set; }

    public bool ProbeInFlight { get; set; }
}

public readonly struct LlmCircuitAdmission
{
    public LlmCircuitAdmission(LlmCircuitSnapshot snapshot, bool rejectPrimary)
    {
        Snapshot = snapshot;
        RejectPrimary = rejectPrimary;
    }

    public LlmCircuitSnapshot Snapshot { get; }

    public bool RejectPrimary { get; }
}

/// <summary>
/// Closed, open, and half-open breaker. The error rate is failures in the recent window.
/// </summary>
public static class LlmCircuitBreaker
{
    public static LlmCircuitAdmission Admit(LlmCircuitSnapshot snapshot, DateTimeOffset now, LlmGatewayPolicy policy)
    {
        var copy = Copy(snapshot);
        if (copy.State == LlmCircuitState.Open)
        {
            if (copy.OpenedAt is DateTimeOffset opened && now < opened + policy.BreakerOpen)
            {
                return new LlmCircuitAdmission(copy, rejectPrimary: true);
            }

            copy.State = LlmCircuitState.HalfOpen;
            copy.ProbeInFlight = false;
        }

        if (copy.State == LlmCircuitState.HalfOpen && copy.ProbeInFlight)
        {
            return new LlmCircuitAdmission(copy, rejectPrimary: true);
        }

        if (copy.State == LlmCircuitState.HalfOpen)
        {
            copy.ProbeInFlight = true;
        }

        return new LlmCircuitAdmission(copy, rejectPrimary: false);
    }

    public static LlmCircuitSnapshot Record(LlmCircuitSnapshot snapshot, bool success, DateTimeOffset now, LlmGatewayPolicy policy)
    {
        var copy = Copy(snapshot);
        if (success)
        {
            copy.State = LlmCircuitState.Closed;
            copy.ProbeInFlight = false;
            copy.OpenedAt = null;
            copy.Failures.Clear();
            return copy;
        }

        copy.ProbeInFlight = false;
        copy.Failures.Add(true);
        var window = Math.Max(1, policy.BreakerWindow);
        if (copy.Failures.Count > window)
        {
            copy.Failures.RemoveRange(0, copy.Failures.Count - window);
        }

        var samples = copy.Failures.Count;
        var rate = samples == 0 ? 0 : copy.Failures.Count(item => item) / (double)samples;
        if (copy.State == LlmCircuitState.HalfOpen ||
            (samples >= Math.Max(1, policy.BreakerMinimumSamples) && rate >= policy.BreakerErrorRate))
        {
            copy.State = LlmCircuitState.Open;
            copy.OpenedAt = now;
        }

        return copy;
    }

    private static LlmCircuitSnapshot Copy(LlmCircuitSnapshot snapshot)
    {
        return new LlmCircuitSnapshot
        {
            State = snapshot.State,
            Failures = snapshot.Failures.ToList(),
            OpenedAt = snapshot.OpenedAt,
            ProbeInFlight = snapshot.ProbeInFlight
        };
    }
}

public interface ILlmCircuitStore
{
    Task<LlmCircuitSnapshot> GetAsync(string key, CancellationToken cancellationToken = default);

    Task SaveAsync(string key, LlmCircuitSnapshot snapshot, CancellationToken cancellationToken = default);
}

/// <summary>
/// Process-local breaker state. The key is tenant, provider, workspace, and model,
/// the same shape as <c>HooshvareRagCircuitStore</c>. A half-open probe stays on this process.
/// </summary>
public class MemoryLlmCircuitStore : ILlmCircuitStore, Volo.Abp.DependencyInjection.ISingletonDependency
{
    private readonly object _gate = new();
    private readonly Dictionary<string, LlmCircuitSnapshot> _items = new(StringComparer.Ordinal);

    public Task<LlmCircuitSnapshot> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            return Task.FromResult(_items.TryGetValue(key, out var snapshot) ? Copy(snapshot) : new LlmCircuitSnapshot());
        }
    }

    public Task SaveAsync(string key, LlmCircuitSnapshot snapshot, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            _items[key] = Copy(snapshot);
            return Task.CompletedTask;
        }
    }

    private static LlmCircuitSnapshot Copy(LlmCircuitSnapshot snapshot)
    {
        return new LlmCircuitSnapshot
        {
            State = snapshot.State,
            Failures = snapshot.Failures.ToList(),
            OpenedAt = snapshot.OpenedAt,
            ProbeInFlight = snapshot.ProbeInFlight
        };
    }
}
