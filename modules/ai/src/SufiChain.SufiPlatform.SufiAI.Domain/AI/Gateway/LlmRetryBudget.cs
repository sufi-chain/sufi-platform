namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// Caps retries at about <see cref="LlmGatewayPolicy.RetryBudgetFraction"/> of requests.
/// The first few requests may still retry so one failing call is not stuck with a zero budget.
/// </summary>
public sealed class LlmRetryBudget : Volo.Abp.DependencyInjection.ISingletonDependency
{
    private readonly object _gate = new();

    public int TotalRequests { get; private set; }

    public int RetriesUsed { get; private set; }

    public void AddRequest()
    {
        lock (_gate)
        {
            TotalRequests++;
        }
    }

    public void Seed(int totalRequests, int retriesUsed)
    {
        lock (_gate)
        {
            TotalRequests = totalRequests;
            RetriesUsed = retriesUsed;
        }
    }

    public bool TryConsume(double fraction)
    {
        lock (_gate)
        {
            if (TotalRequests < 10)
            {
                RetriesUsed++;
                return true;
            }

            if (RetriesUsed + 1 <= TotalRequests * fraction)
            {
                RetriesUsed++;
                return true;
            }

            return false;
        }
    }
}

public static class LlmFullJitter
{
    public static TimeSpan Delay(int retryOrdinal, LlmGatewayPolicy policy, double unitRandom)
    {
        var unit = Math.Clamp(unitRandom, 0d, 1d);
        var exponential = policy.BaseDelayMs * Math.Pow(2, Math.Max(0, retryOrdinal));
        var cap = Math.Min(policy.MaxDelayMs, exponential);
        if (cap <= 0)
        {
            return TimeSpan.Zero;
        }

        return TimeSpan.FromMilliseconds(unit * cap);
    }
}
