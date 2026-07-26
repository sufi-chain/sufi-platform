using Microsoft.Extensions.Caching.Distributed;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.EventBus;

/// <summary>
/// Cache-backed idempotency store for distributed event handlers.
/// </summary>
public class DistributedEventIdempotencyStore : IDistributedEventIdempotencyStore, ITransientDependency
{
    protected IDistributedCache Cache { get; }
    protected ICurrentTenant CurrentTenant { get; }

    public DistributedEventIdempotencyStore(IDistributedCache cache, ICurrentTenant currentTenant)
    {
        Cache = cache;
        CurrentTenant = currentTenant;
    }

    public virtual async Task<bool> TryBeginAsync(
        Guid eventId,
        Guid? tenantId,
        TimeSpan? ttl = null,
        CancellationToken cancellationToken = default,
        string? handlerKey = null)
    {
        var key = BuildKey(eventId, tenantId ?? CurrentTenant.Id, handlerKey);
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = ttl ?? TimeSpan.FromDays(7)
        };

        // Get+Set is not atomic across all providers; for Phase 0 this is sufficient
        // inside the monolith Inbox which already tracks IncomingEventRecord.
        var existing = await Cache.GetStringAsync(key, cancellationToken);
        if (!string.IsNullOrEmpty(existing))
        {
            return false;
        }

        await Cache.SetStringAsync(key, "1", options, cancellationToken);
        return true;
    }

    protected virtual string BuildKey(Guid eventId, Guid? tenantId, string? handlerKey)
    {
        var scope = string.IsNullOrWhiteSpace(handlerKey) ? "default" : handlerKey.Trim();
        return $"Sufi:EventIdempotency:{(tenantId?.ToString() ?? "host")}:{scope}:{eventId:N}";
    }
}
