using Microsoft.Extensions.Caching.Distributed;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Volo.Abp.DistributedLocking;

namespace SufiChain.SufiPlatform.SufiAI.Catalog;

/// <summary>
/// Reads a typed cache entry and lets one instance refresh it.
/// A null lock handle means another instance is already refreshing.
/// </summary>
public class DistributedRefreshGate : ITransientDependency
{
    public const int NegativeSeconds = 60;

    private readonly IAbpDistributedLock _distributedLock;

    public DistributedRefreshGate(IAbpDistributedLock distributedLock)
    {
        _distributedLock = distributedLock;
    }

    public async Task<T?> ReadOrRefreshAsync<T>(
        IDistributedCache<T> cache,
        string cacheKey,
        string lockName,
        int ttlSeconds,
        Func<Task<T?>> refresh,
        Func<T, bool>? isNegative = null,
        CancellationToken cancellationToken = default)
        where T : class
    {
        if (ttlSeconds <= 0)
        {
            return await refresh();
        }

        var cached = await cache.GetAsync(cacheKey, token: cancellationToken);
        if (cached != null)
        {
            return cached;
        }

        await using var handle = await _distributedLock.TryAcquireAsync(
            lockName,
            TimeSpan.FromSeconds(15),
            cancellationToken);
        if (handle == null)
        {
            return await cache.GetAsync(cacheKey, token: cancellationToken);
        }

        cached = await cache.GetAsync(cacheKey, token: cancellationToken);
        if (cached != null)
        {
            return cached;
        }

        var fresh = await refresh();
        if (fresh == null)
        {
            return null;
        }

        var negative = isNegative?.Invoke(fresh) == true;
        await cache.SetAsync(
            cacheKey,
            fresh,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(negative ? NegativeSeconds : ttlSeconds)
            },
            token: cancellationToken);
        return fresh;
    }

    public Task<IAbpDistributedLockHandle?> TryAcquireAsync(string lockName, CancellationToken cancellationToken = default)
    {
        return _distributedLock.TryAcquireAsync(lockName, TimeSpan.FromSeconds(15), cancellationToken);
    }
}
