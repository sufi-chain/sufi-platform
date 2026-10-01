using Microsoft.Extensions.Caching.Distributed;
using Volo.Abp.Caching;

namespace SufiChain.SufiPlatform.Caching;

/// <summary>
/// Hides cached values while a temporary revocation is active. Writes still go through,
/// so a value stored during the window is served again after the deadline.
/// </summary>
public class RevocableDistributedCache<TCacheItem, TCacheKey> : IDistributedCache<TCacheItem, TCacheKey>
    where TCacheItem : class
    where TCacheKey : notnull
{
    private readonly DistributedCache<TCacheItem, TCacheKey> _inner;
    private readonly ICacheRevocationStore _revocations;
    private readonly string _cacheName;

    public RevocableDistributedCache(
        DistributedCache<TCacheItem, TCacheKey> inner,
        ICacheRevocationStore revocations)
    {
        _inner = inner;
        _revocations = revocations;
        _cacheName = CacheNameAttribute.GetCacheName(typeof(TCacheItem));
    }

    public TCacheItem? Get(TCacheKey key, bool? hideErrors = null, bool considerUow = false)
    {
        return IsRevoked(key) ? null : _inner.Get(key, hideErrors, considerUow);
    }

    public Task<TCacheItem?> GetAsync(
        TCacheKey key,
        bool? hideErrors = null,
        bool considerUow = false,
        CancellationToken token = default)
    {
        return IsRevoked(key) ? Task.FromResult<TCacheItem?>(null) : _inner.GetAsync(key, hideErrors, considerUow, token);
    }

    public KeyValuePair<TCacheKey, TCacheItem?>[] GetMany(
        IEnumerable<TCacheKey> keys,
        bool? hideErrors = null,
        bool considerUow = false)
    {
        return ReadMany(keys.ToList(), hideErrors, considerUow, static (inner, allowed, hide, uow) => inner.GetMany(allowed, hide, uow));
    }

    public Task<KeyValuePair<TCacheKey, TCacheItem?>[]> GetManyAsync(
        IEnumerable<TCacheKey> keys,
        bool? hideErrors = null,
        bool considerUow = false,
        CancellationToken token = default)
    {
        var list = keys.ToList();
        if (!list.Any(IsRevoked))
        {
            return _inner.GetManyAsync(list, hideErrors, considerUow, token);
        }

        return ReadManyAsync(list, hideErrors, considerUow, token);
    }

    public TCacheItem? GetOrAdd(
        TCacheKey key,
        Func<TCacheItem> factory,
        Func<DistributedCacheEntryOptions>? optionsFactory = null,
        bool? hideErrors = null,
        bool considerUow = false)
    {
        if (!IsRevoked(key))
        {
            return _inner.GetOrAdd(key, factory, optionsFactory, hideErrors, considerUow);
        }

        var value = factory();
        if (value != null)
        {
            _inner.Set(key, value, optionsFactory?.Invoke(), hideErrors, considerUow);
        }

        return value;
    }

    public async Task<TCacheItem?> GetOrAddAsync(
        TCacheKey key,
        Func<Task<TCacheItem>> factory,
        Func<DistributedCacheEntryOptions>? optionsFactory = null,
        bool? hideErrors = null,
        bool considerUow = false,
        CancellationToken token = default)
    {
        if (!IsRevoked(key))
        {
            return await _inner.GetOrAddAsync(key, factory, optionsFactory, hideErrors, considerUow, token);
        }

        var value = await factory();
        if (value != null)
        {
            await _inner.SetAsync(key, value, optionsFactory?.Invoke(), hideErrors, considerUow, token);
        }

        return value;
    }

    public KeyValuePair<TCacheKey, TCacheItem?>[] GetOrAddMany(
        IEnumerable<TCacheKey> keys,
        Func<IEnumerable<TCacheKey>, List<KeyValuePair<TCacheKey, TCacheItem>>> factory,
        Func<DistributedCacheEntryOptions>? optionsFactory = null,
        bool? hideErrors = null,
        bool considerUow = false)
    {
        var list = keys.ToList();
        if (!list.Any(IsRevoked))
        {
            return _inner.GetOrAddMany(list, factory, optionsFactory, hideErrors, considerUow);
        }

        var current = GetMany(list, hideErrors, considerUow);
        var missing = current.Where(item => item.Value == null).Select(item => item.Key).ToList();
        if (missing.Count == 0)
        {
            return current;
        }

        var created = factory(missing);
        if (created.Count > 0)
        {
            _inner.SetMany(created, optionsFactory?.Invoke(), hideErrors, considerUow);
        }

        return Merge(list, current, created);
    }

    public async Task<KeyValuePair<TCacheKey, TCacheItem?>[]> GetOrAddManyAsync(
        IEnumerable<TCacheKey> keys,
        Func<IEnumerable<TCacheKey>, Task<List<KeyValuePair<TCacheKey, TCacheItem>>>> factory,
        Func<DistributedCacheEntryOptions>? optionsFactory = null,
        bool? hideErrors = null,
        bool considerUow = false,
        CancellationToken token = default)
    {
        var list = keys.ToList();
        if (!list.Any(IsRevoked))
        {
            return await _inner.GetOrAddManyAsync(list, factory, optionsFactory, hideErrors, considerUow, token);
        }

        var current = await GetManyAsync(list, hideErrors, considerUow, token);
        var missing = current.Where(item => item.Value == null).Select(item => item.Key).ToList();
        if (missing.Count == 0)
        {
            return current;
        }

        var created = await factory(missing);
        if (created.Count > 0)
        {
            await _inner.SetManyAsync(created, optionsFactory?.Invoke(), hideErrors, considerUow, token);
        }

        return Merge(list, current, created);
    }

    public void Set(
        TCacheKey key,
        TCacheItem value,
        DistributedCacheEntryOptions? options = null,
        bool? hideErrors = null,
        bool considerUow = false)
    {
        _inner.Set(key, value, options, hideErrors, considerUow);
    }

    public Task SetAsync(
        TCacheKey key,
        TCacheItem value,
        DistributedCacheEntryOptions? options = null,
        bool? hideErrors = null,
        bool considerUow = false,
        CancellationToken token = default)
    {
        return _inner.SetAsync(key, value, options, hideErrors, considerUow, token);
    }

    public void SetMany(
        IEnumerable<KeyValuePair<TCacheKey, TCacheItem>> items,
        DistributedCacheEntryOptions? options = null,
        bool? hideErrors = null,
        bool considerUow = false)
    {
        _inner.SetMany(items, options, hideErrors, considerUow);
    }

    public Task SetManyAsync(
        IEnumerable<KeyValuePair<TCacheKey, TCacheItem>> items,
        DistributedCacheEntryOptions? options = null,
        bool? hideErrors = null,
        bool considerUow = false,
        CancellationToken token = default)
    {
        return _inner.SetManyAsync(items, options, hideErrors, considerUow, token);
    }

    public void Refresh(TCacheKey key, bool? hideErrors = null)
    {
        _inner.Refresh(key, hideErrors);
    }

    public Task RefreshAsync(TCacheKey key, bool? hideErrors = null, CancellationToken token = default)
    {
        return _inner.RefreshAsync(key, hideErrors, token);
    }

    public void RefreshMany(IEnumerable<TCacheKey> keys, bool? hideErrors = null)
    {
        _inner.RefreshMany(keys, hideErrors);
    }

    public Task RefreshManyAsync(IEnumerable<TCacheKey> keys, bool? hideErrors = null, CancellationToken token = default)
    {
        return _inner.RefreshManyAsync(keys, hideErrors, token);
    }

    public void Remove(TCacheKey key, bool? hideErrors = null, bool considerUow = false)
    {
        _inner.Remove(key, hideErrors, considerUow);
    }

    public Task RemoveAsync(TCacheKey key, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default)
    {
        return _inner.RemoveAsync(key, hideErrors, considerUow, token);
    }

    public void RemoveMany(IEnumerable<TCacheKey> keys, bool? hideErrors = null, bool considerUow = false)
    {
        _inner.RemoveMany(keys, hideErrors, considerUow);
    }

    public Task RemoveManyAsync(
        IEnumerable<TCacheKey> keys,
        bool? hideErrors = null,
        bool considerUow = false,
        CancellationToken token = default)
    {
        return _inner.RemoveManyAsync(keys, hideErrors, considerUow, token);
    }

    private bool IsRevoked(TCacheKey key)
    {
        return _revocations.IsRevoked(_cacheName, key.ToString());
    }

    private KeyValuePair<TCacheKey, TCacheItem?>[] ReadMany(
        List<TCacheKey> keys,
        bool? hideErrors,
        bool considerUow,
        Func<DistributedCache<TCacheItem, TCacheKey>, IEnumerable<TCacheKey>, bool?, bool, KeyValuePair<TCacheKey, TCacheItem?>[]> read)
    {
        if (!keys.Any(IsRevoked))
        {
            return read(_inner, keys, hideErrors, considerUow);
        }

        var allowed = keys.Where(key => !IsRevoked(key)).ToList();
        var fetched = allowed.Count == 0
            ? Array.Empty<KeyValuePair<TCacheKey, TCacheItem?>>()
            : read(_inner, allowed, hideErrors, considerUow);
        return Project(keys, fetched);
    }

    private async Task<KeyValuePair<TCacheKey, TCacheItem?>[]> ReadManyAsync(
        List<TCacheKey> keys,
        bool? hideErrors,
        bool considerUow,
        CancellationToken token)
    {
        var allowed = keys.Where(key => !IsRevoked(key)).ToList();
        var fetched = allowed.Count == 0
            ? Array.Empty<KeyValuePair<TCacheKey, TCacheItem?>>()
            : await _inner.GetManyAsync(allowed, hideErrors, considerUow, token);
        return Project(keys, fetched);
    }

    private KeyValuePair<TCacheKey, TCacheItem?>[] Project(
        List<TCacheKey> keys,
        KeyValuePair<TCacheKey, TCacheItem?>[] fetched)
    {
        var values = new Dictionary<TCacheKey, TCacheItem?>(fetched.Length);
        foreach (var item in fetched)
        {
            values[item.Key] = item.Value;
        }

        var result = new KeyValuePair<TCacheKey, TCacheItem?>[keys.Count];
        for (var index = 0; index < keys.Count; index++)
        {
            var key = keys[index];
            values.TryGetValue(key, out var value);
            result[index] = new KeyValuePair<TCacheKey, TCacheItem?>(key, IsRevoked(key) ? null : value);
        }

        return result;
    }

    private static KeyValuePair<TCacheKey, TCacheItem?>[] Merge(
        List<TCacheKey> keys,
        KeyValuePair<TCacheKey, TCacheItem?>[] current,
        List<KeyValuePair<TCacheKey, TCacheItem>> created)
    {
        var values = new Dictionary<TCacheKey, TCacheItem?>(keys.Count);
        foreach (var item in current)
        {
            values[item.Key] = item.Value;
        }

        foreach (var item in created)
        {
            values[item.Key] = item.Value;
        }

        var result = new KeyValuePair<TCacheKey, TCacheItem?>[keys.Count];
        for (var index = 0; index < keys.Count; index++)
        {
            var key = keys[index];
            values.TryGetValue(key, out var value);
            result[index] = new KeyValuePair<TCacheKey, TCacheItem?>(key, value);
        }

        return result;
    }
}
