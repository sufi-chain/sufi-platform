using Microsoft.Extensions.Caching.Distributed;
using Volo.Abp.Caching;

namespace SufiChain.SufiPlatform.Settings;

internal sealed class RecordingDistributedCache<TCacheItem> : IDistributedCache<TCacheItem>
    where TCacheItem : class
{
    private readonly Dictionary<string, TCacheItem> _items = new(StringComparer.Ordinal);

    public List<CacheWrite<TCacheItem>> Writes { get; } = new();

    public List<CacheRemove> Removes { get; } = new();

    public IDistributedCache<TCacheItem, string> InternalCache => this;

    public TCacheItem? Get(string key, bool? hideErrors = null, bool considerUow = false)
    {
        return GetAsync(key, hideErrors, considerUow).GetAwaiter().GetResult();
    }

    public Task<TCacheItem?> GetAsync(
        string key,
        bool? hideErrors = null,
        bool considerUow = false,
        CancellationToken token = default)
    {
        _items.TryGetValue(key, out var value);
        return Task.FromResult(value);
    }

    public KeyValuePair<string, TCacheItem?>[] GetMany(
        IEnumerable<string> keys,
        bool? hideErrors = null,
        bool considerUow = false)
    {
        return GetManyAsync(keys, hideErrors, considerUow).GetAwaiter().GetResult();
    }

    public Task<KeyValuePair<string, TCacheItem?>[]> GetManyAsync(
        IEnumerable<string> keys,
        bool? hideErrors = null,
        bool considerUow = false,
        CancellationToken token = default)
    {
        var result = keys.Select(key =>
        {
            _items.TryGetValue(key, out var value);
            return new KeyValuePair<string, TCacheItem?>(key, value);
        }).ToArray();
        return Task.FromResult(result);
    }

    public TCacheItem? GetOrAdd(
        string key,
        Func<TCacheItem> factory,
        Func<DistributedCacheEntryOptions>? optionsFactory = null,
        bool? hideErrors = null,
        bool considerUow = false)
    {
        throw new NotSupportedException();
    }

    public Task<TCacheItem?> GetOrAddAsync(
        string key,
        Func<Task<TCacheItem>> factory,
        Func<DistributedCacheEntryOptions>? optionsFactory = null,
        bool? hideErrors = null,
        bool considerUow = false,
        CancellationToken token = default)
    {
        throw new NotSupportedException();
    }

    public KeyValuePair<string, TCacheItem?>[] GetOrAddMany(
        IEnumerable<string> keys,
        Func<IEnumerable<string>, List<KeyValuePair<string, TCacheItem>>> factory,
        Func<DistributedCacheEntryOptions>? optionsFactory = null,
        bool? hideErrors = null,
        bool considerUow = false)
    {
        throw new NotSupportedException();
    }

    public Task<KeyValuePair<string, TCacheItem?>[]> GetOrAddManyAsync(
        IEnumerable<string> keys,
        Func<IEnumerable<string>, Task<List<KeyValuePair<string, TCacheItem>>>> factory,
        Func<DistributedCacheEntryOptions>? optionsFactory = null,
        bool? hideErrors = null,
        bool considerUow = false,
        CancellationToken token = default)
    {
        throw new NotSupportedException();
    }

    public void Set(
        string key,
        TCacheItem value,
        DistributedCacheEntryOptions? options = null,
        bool? hideErrors = null,
        bool considerUow = false)
    {
        SetAsync(key, value, options, hideErrors, considerUow).GetAwaiter().GetResult();
    }

    public Task SetAsync(
        string key,
        TCacheItem value,
        DistributedCacheEntryOptions? options = null,
        bool? hideErrors = null,
        bool considerUow = false,
        CancellationToken token = default)
    {
        _items[key] = value;
        Writes.Add(new CacheWrite<TCacheItem>(key, value, options, considerUow));
        return Task.CompletedTask;
    }

    public void SetMany(
        IEnumerable<KeyValuePair<string, TCacheItem>> items,
        DistributedCacheEntryOptions? options = null,
        bool? hideErrors = null,
        bool considerUow = false)
    {
        SetManyAsync(items, options, hideErrors, considerUow).GetAwaiter().GetResult();
    }

    public Task SetManyAsync(
        IEnumerable<KeyValuePair<string, TCacheItem>> items,
        DistributedCacheEntryOptions? options = null,
        bool? hideErrors = null,
        bool considerUow = false,
        CancellationToken token = default)
    {
        foreach (var item in items)
        {
            _items[item.Key] = item.Value;
            Writes.Add(new CacheWrite<TCacheItem>(item.Key, item.Value, options, considerUow));
        }

        return Task.CompletedTask;
    }

    public void Refresh(string key, bool? hideErrors = null)
    {
    }

    public Task RefreshAsync(string key, bool? hideErrors = null, CancellationToken token = default)
    {
        return Task.CompletedTask;
    }

    public void RefreshMany(IEnumerable<string> keys, bool? hideErrors = null)
    {
    }

    public Task RefreshManyAsync(IEnumerable<string> keys, bool? hideErrors = null, CancellationToken token = default)
    {
        return Task.CompletedTask;
    }

    public void Remove(string key, bool? hideErrors = null, bool considerUow = false)
    {
        RemoveAsync(key, hideErrors, considerUow).GetAwaiter().GetResult();
    }

    public Task RemoveAsync(string key, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default)
    {
        _items.Remove(key);
        Removes.Add(new CacheRemove(key, considerUow));
        return Task.CompletedTask;
    }

    public void RemoveMany(IEnumerable<string> keys, bool? hideErrors = null, bool considerUow = false)
    {
        RemoveManyAsync(keys, hideErrors, considerUow).GetAwaiter().GetResult();
    }

    public Task RemoveManyAsync(
        IEnumerable<string> keys,
        bool? hideErrors = null,
        bool considerUow = false,
        CancellationToken token = default)
    {
        foreach (var key in keys)
        {
            _items.Remove(key);
            Removes.Add(new CacheRemove(key, considerUow));
        }

        return Task.CompletedTask;
    }
}

internal sealed record CacheWrite<TCacheItem>(
    string Key,
    TCacheItem Value,
    DistributedCacheEntryOptions? Options,
    bool ConsiderUow);

internal sealed record CacheRemove(string Key, bool ConsiderUow);
