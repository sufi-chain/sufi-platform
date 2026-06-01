using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;

namespace SufiChain.Chat.Usage;

public class ChatRateLimitCounterStore : IChatRateLimitCounterStore, ITransientDependency
{
    protected IDistributedCache<ChatRateLimitCounterCacheItem> Cache { get; }

    public ChatRateLimitCounterStore(IDistributedCache<ChatRateLimitCounterCacheItem> cache)
    {
        Cache = cache;
    }

    public virtual async Task<long> IncrementAsync(
        string key,
        TimeSpan window,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = "Chat:RateLimit:" + key;
        var item = await Cache.GetAsync(cacheKey, token: cancellationToken) ?? new ChatRateLimitCounterCacheItem();
        item.Count++;

        await Cache.SetAsync(
            cacheKey,
            item,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = window
            },
            token: cancellationToken);

        return item.Count;
    }
}
