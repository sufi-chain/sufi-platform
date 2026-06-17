using Microsoft.Extensions.Caching.Distributed;
using SufiChain.SufiAbp.DependencyInjection;

namespace SufiChain.SufiAbp.Caching;

public class SufiAbpDistributedCache<TCacheItem> : IDistributedCache<TCacheItem>, ITransientDependency
    where TCacheItem : class
{
     protected Volo.Abp.Caching.IDistributedCache<TCacheItem, string> InternalCache { get; }

    Volo.Abp.Caching.IDistributedCache<TCacheItem, string> Volo.Abp.Caching.IDistributedCache<TCacheItem>.InternalCache => InternalCache;

    public SufiAbpDistributedCache(Volo.Abp.Caching.IDistributedCache<TCacheItem, string> internalCache)
     {
         InternalCache = internalCache;
     }

    public virtual TCacheItem? Get(string key, bool? hideErrors = null, bool considerUow = false)
    {
        return InternalCache.Get(key, hideErrors, considerUow);
    }

    public virtual KeyValuePair<string, TCacheItem?>[] GetMany(IEnumerable<string> keys, bool? hideErrors = null, bool considerUow = false)
    {
        return InternalCache.GetMany(keys, hideErrors, considerUow);
    }

    public virtual Task<KeyValuePair<string, TCacheItem?>[]> GetManyAsync(IEnumerable<string> keys, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default)
    {
        return InternalCache.GetManyAsync(keys, hideErrors, considerUow, token);
    }

    public virtual Task<TCacheItem?> GetAsync(string key, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default)
    {
        return InternalCache.GetAsync(key, hideErrors, considerUow, token);
    }

    public virtual TCacheItem? GetOrAdd(string key, Func<TCacheItem> factory, Func<DistributedCacheEntryOptions>? optionsFactory = null, bool? hideErrors = null, bool considerUow = false)
    {
        return InternalCache.GetOrAdd(key, factory, optionsFactory, hideErrors, considerUow);
    }

    public virtual Task<TCacheItem?> GetOrAddAsync(string key, Func<Task<TCacheItem>> factory, Func<DistributedCacheEntryOptions>? optionsFactory = null, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default)
    {
        return InternalCache.GetOrAddAsync(key, factory, optionsFactory, hideErrors, considerUow, token);
    }

    public virtual KeyValuePair<string, TCacheItem?>[] GetOrAddMany(IEnumerable<string> keys, Func<IEnumerable<string>, List<KeyValuePair<string, TCacheItem>>> factory, Func<DistributedCacheEntryOptions>? optionsFactory = null, bool? hideErrors = null, bool considerUow = false)
    {
        return InternalCache.GetOrAddMany(keys, factory, optionsFactory, hideErrors, considerUow);
    }

    public virtual Task<KeyValuePair<string, TCacheItem?>[]> GetOrAddManyAsync(IEnumerable<string> keys, Func<IEnumerable<string>, Task<List<KeyValuePair<string, TCacheItem>>>> factory, Func<DistributedCacheEntryOptions>? optionsFactory = null, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default)
    {
        return InternalCache.GetOrAddManyAsync(keys, factory, optionsFactory, hideErrors, considerUow, token);
    }

    public virtual void Set(string key, TCacheItem value, DistributedCacheEntryOptions? options = null, bool? hideErrors = null, bool considerUow = false)
    {
        InternalCache.Set(key, value, options, hideErrors, considerUow);
    }

    public virtual Task SetAsync(string key, TCacheItem value, DistributedCacheEntryOptions? options = null, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default)
    {
        return InternalCache.SetAsync(key, value, options, hideErrors, considerUow, token);
    }

    public virtual void SetMany(IEnumerable<KeyValuePair<string, TCacheItem>> items, DistributedCacheEntryOptions? options = null, bool? hideErrors = null, bool considerUow = false)
    {
        InternalCache.SetMany(items, options, hideErrors, considerUow);
    }

    public virtual Task SetManyAsync(IEnumerable<KeyValuePair<string, TCacheItem>> items, DistributedCacheEntryOptions? options = null, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default)
    {
        return InternalCache.SetManyAsync(items, options, hideErrors, considerUow, token);
    }

    public virtual void Refresh(string key, bool? hideErrors = null)
    {
        InternalCache.Refresh(key, hideErrors);
    }

    public virtual Task RefreshAsync(string key, bool? hideErrors = null, CancellationToken token = default)
    {
        return InternalCache.RefreshAsync(key, hideErrors, token);
    }

    public virtual void RefreshMany(IEnumerable<string> keys, bool? hideErrors = null)
    {
        InternalCache.RefreshMany(keys, hideErrors);
    }

    public virtual Task RefreshManyAsync(IEnumerable<string> keys, bool? hideErrors = null, CancellationToken token = default)
    {
        return InternalCache.RefreshManyAsync(keys, hideErrors, token);
    }

    public virtual void Remove(string key, bool? hideErrors = null, bool considerUow = false)
    {
        InternalCache.Remove(key, hideErrors, considerUow);
    }

    public virtual Task RemoveAsync(string key, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default)
    {
        return InternalCache.RemoveAsync(key, hideErrors, considerUow, token);
    }

    public virtual void RemoveMany(IEnumerable<string> keys, bool? hideErrors = null, bool considerUow = false)
    {
        InternalCache.RemoveMany(keys, hideErrors, considerUow);
    }

    public virtual Task RemoveManyAsync(IEnumerable<string> keys, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default)
    {
        return InternalCache.RemoveManyAsync(keys, hideErrors, considerUow, token);
    }
}
