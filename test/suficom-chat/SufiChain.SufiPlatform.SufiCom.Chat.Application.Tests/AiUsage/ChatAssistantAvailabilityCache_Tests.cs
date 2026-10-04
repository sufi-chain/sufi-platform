using Microsoft.Extensions.Caching.Distributed;
using Shouldly;
using SufiChain.SufiPlatform.Features;
using SufiChain.SufiPlatform.Settings;
using SufiChain.SufiPlatform.SufiCom.Chat.AiUsage;
using SufiChain.SufiPlatform.SufiCom.Chat.Settings;
using Volo.Abp.Caching;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.Settings;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Chat.AiUsage;

public class ChatAssistantAvailabilityCache_Tests
{
    [Fact]
    public async Task GetOrAdd_returns_the_cached_snapshot_until_it_is_invalidated()
    {
        var cache = CreateCache();
        var calls = 0;

        Task<ChatAssistantAvailabilityDto> Factory()
        {
            calls++;
            return Task.FromResult(Snapshot(calls == 1));
        }

        var first = await cache.GetOrAddAsync(null, Factory);
        var second = await cache.GetOrAddAsync(null, Factory);

        first.IsAvailable.ShouldBeTrue();
        second.IsAvailable.ShouldBeTrue();
        calls.ShouldBe(1);

        await cache.InvalidateAsync(null, hostWide: true);
        var third = await cache.GetOrAddAsync(null, Factory);

        third.IsAvailable.ShouldBeFalse();
        calls.ShouldBe(2);
    }

    [Fact]
    public async Task Host_invalidation_drops_a_tenant_snapshot_without_sharing_it()
    {
        var cache = CreateCache();
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var calls = new Dictionary<string, int>();

        Task<ChatAssistantAvailabilityDto> Factory(Guid? tenantId)
        {
            var scope = tenantId?.ToString("D") ?? "host";
            calls[scope] = calls.GetValueOrDefault(scope) + 1;
            return Task.FromResult(Snapshot(available: true, scope));
        }

        await cache.GetOrAddAsync(tenantA, () => Factory(tenantA));
        await cache.GetOrAddAsync(tenantB, () => Factory(tenantB));
        await cache.GetOrAddAsync(null, () => Factory(null));

        await cache.InvalidateAsync(null, hostWide: true);

        await cache.GetOrAddAsync(tenantA, () => Factory(tenantA));
        await cache.GetOrAddAsync(tenantB, () => Factory(tenantB));
        await cache.GetOrAddAsync(null, () => Factory(null));

        calls[tenantA.ToString("D")].ShouldBe(2);
        calls[tenantB.ToString("D")].ShouldBe(2);
        calls["host"].ShouldBe(2);
    }

    [Fact]
    public async Task Tenant_invalidation_leaves_other_tenants_and_the_host_cached()
    {
        var cache = CreateCache();
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var calls = new Dictionary<string, int>();

        Task<ChatAssistantAvailabilityDto> Factory(Guid? tenantId)
        {
            var scope = tenantId?.ToString("D") ?? "host";
            calls[scope] = calls.GetValueOrDefault(scope) + 1;
            return Task.FromResult(Snapshot(available: true));
        }

        await cache.GetOrAddAsync(tenantA, () => Factory(tenantA));
        await cache.GetOrAddAsync(tenantB, () => Factory(tenantB));
        await cache.GetOrAddAsync(null, () => Factory(null));

        await cache.InvalidateAsync(tenantA, hostWide: false);

        await cache.GetOrAddAsync(tenantA, () => Factory(tenantA));
        await cache.GetOrAddAsync(tenantB, () => Factory(tenantB));
        await cache.GetOrAddAsync(null, () => Factory(null));

        calls[tenantA.ToString("D")].ShouldBe(2);
        calls[tenantB.ToString("D")].ShouldBe(1);
        calls["host"].ShouldBe(1);
    }

    [Fact]
    public async Task Cache_key_contains_the_tenant_id_and_entries_expire()
    {
        var availability = new RecordingDistributedCache<ChatAssistantAvailabilityCacheItem>();
        var stamps = new RecordingDistributedCache<ChatAssistantAvailabilityStampCacheItem>();
        var cache = new ChatAssistantAvailabilityCache(availability, stamps);
        var tenantId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");

        await cache.GetOrAddAsync(tenantId, () => Task.FromResult(Snapshot(true)));

        var key = await cache.BuildKeyAsync(tenantId);
        key.ShouldContain("t:" + tenantId.ToString("D"));
        key.ShouldNotContain(ChatAssistantAvailabilityCache.HostScope);
        (await cache.BuildKeyAsync(null)).ShouldContain(":" + ChatAssistantAvailabilityCache.HostScope + ":");
        availability.Writes.ShouldContain(write =>
            write.Key == key
            && !write.ConsiderUow
            && write.Options != null
            && write.Options.AbsoluteExpirationRelativeToNow == ChatAssistantAvailabilityCache.AvailabilityTtl);
        ChatAssistantAvailabilityCache.CreateAvailabilityOptions()
            .AbsoluteExpirationRelativeToNow.ShouldBe(TimeSpan.FromMinutes(2));
    }

    [Fact]
    public async Task Setting_changed_event_invalidates_only_assistant_settings()
    {
        var cache = CreateCache();
        var invalidator = new ChatAssistantAvailabilityInvalidator(cache);
        var calls = 0;

        Task<ChatAssistantAvailabilityDto> Factory()
        {
            calls++;
            return Task.FromResult(Snapshot(true));
        }

        await cache.GetOrAddAsync(null, Factory);
        await invalidator.HandleEventAsync(Changed(new Setting(
            Guid.NewGuid(),
            "Chat.Theme",
            "dark",
            GlobalSettingValueProvider.ProviderName,
            null)));
        await cache.GetOrAddAsync(null, Factory);
        calls.ShouldBe(1);

        await invalidator.HandleEventAsync(Changed(new Setting(
            Guid.NewGuid(),
            ChatSettingNames.Ai.Enabled,
            "true",
            GlobalSettingValueProvider.ProviderName,
            null)));
        await cache.GetOrAddAsync(null, Factory);
        calls.ShouldBe(2);

        await invalidator.HandleEventAsync(Changed(new Setting(
            Guid.NewGuid(),
            ChatSettingNames.Ai.ResponseWaitSeconds,
            "45",
            GlobalSettingValueProvider.ProviderName,
            null)));
        await cache.GetOrAddAsync(null, Factory);
        calls.ShouldBe(3);
    }

    [Fact]
    public async Task Feature_change_is_tenant_scoped_unless_the_provider_is_host_wide()
    {
        var cache = CreateCache();
        var invalidator = new ChatAssistantAvailabilityInvalidator(cache);
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var calls = new Dictionary<string, int>();

        Task<ChatAssistantAvailabilityDto> Factory(Guid? tenantId)
        {
            var scope = tenantId?.ToString("D") ?? "host";
            calls[scope] = calls.GetValueOrDefault(scope) + 1;
            return Task.FromResult(Snapshot(true));
        }

        await cache.GetOrAddAsync(tenantA, () => Factory(tenantA));
        await cache.GetOrAddAsync(tenantB, () => Factory(tenantB));

        await invalidator.HandleEventAsync(Changed(new FeatureValue(
            Guid.NewGuid(),
            "Chat.Ai.Enable",
            "true",
            TenantFeatureValueProvider.ProviderName,
            tenantA.ToString("D"))));

        await cache.GetOrAddAsync(tenantA, () => Factory(tenantA));
        await cache.GetOrAddAsync(tenantB, () => Factory(tenantB));
        calls[tenantA.ToString("D")].ShouldBe(2);
        calls[tenantB.ToString("D")].ShouldBe(1);

        await invalidator.HandleEventAsync(Changed(new FeatureValue(
            Guid.NewGuid(),
            "Chat.Ai.Enable",
            "true",
            EditionFeatureValueProvider.ProviderName,
            Guid.NewGuid().ToString("D"))));

        await cache.GetOrAddAsync(tenantA, () => Factory(tenantA));
        await cache.GetOrAddAsync(tenantB, () => Factory(tenantB));
        calls[tenantA.ToString("D")].ShouldBe(3);
        calls[tenantB.ToString("D")].ShouldBe(2);
    }

    private static ChatAssistantAvailabilityCache CreateCache()
    {
        return new ChatAssistantAvailabilityCache(
            new RecordingDistributedCache<ChatAssistantAvailabilityCacheItem>(),
            new RecordingDistributedCache<ChatAssistantAvailabilityStampCacheItem>());
    }

    private static ChatAssistantAvailabilityDto Snapshot(bool available, string? workspace = null)
    {
        return new ChatAssistantAvailabilityDto
        {
            IsAvailable = available,
            DefaultWorkspaceName = workspace
        };
    }

    private static EntityChangedEventData<TEntity> Changed<TEntity>(TEntity entity)
        where TEntity : class
    {
        return new EntityChangedEventData<TEntity>(entity);
    }
}

internal sealed class RecordingDistributedCache<TCacheItem> : IDistributedCache<TCacheItem>
    where TCacheItem : class
{
    private readonly Dictionary<string, TCacheItem> _items = new(StringComparer.Ordinal);

    public List<CacheWrite<TCacheItem>> Writes { get; } = new();

    public IDistributedCache<TCacheItem, string> InternalCache => this;

    public TCacheItem? Get(string key, bool? hideErrors = null, bool considerUow = false)
    {
        return GetAsync(key, hideErrors, considerUow).GetAwaiter().GetResult();
    }

    public Task<TCacheItem?> GetAsync(string key, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default)
    {
        _items.TryGetValue(key, out var value);
        return Task.FromResult(value);
    }

    public KeyValuePair<string, TCacheItem?>[] GetMany(IEnumerable<string> keys, bool? hideErrors = null, bool considerUow = false)
    {
        return keys.Select(key => new KeyValuePair<string, TCacheItem?>(key, _items.GetValueOrDefault(key))).ToArray();
    }

    public Task<KeyValuePair<string, TCacheItem?>[]> GetManyAsync(IEnumerable<string> keys, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default)
    {
        return Task.FromResult(GetMany(keys, hideErrors, considerUow));
    }

    public TCacheItem? GetOrAdd(string key, Func<TCacheItem> factory, Func<DistributedCacheEntryOptions>? optionsFactory = null, bool? hideErrors = null, bool considerUow = false) => throw new NotSupportedException();

    public Task<TCacheItem?> GetOrAddAsync(string key, Func<Task<TCacheItem>> factory, Func<DistributedCacheEntryOptions>? optionsFactory = null, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default) => throw new NotSupportedException();

    public KeyValuePair<string, TCacheItem?>[] GetOrAddMany(IEnumerable<string> keys, Func<IEnumerable<string>, List<KeyValuePair<string, TCacheItem>>> factory, Func<DistributedCacheEntryOptions>? optionsFactory = null, bool? hideErrors = null, bool considerUow = false) => throw new NotSupportedException();

    public Task<KeyValuePair<string, TCacheItem?>[]> GetOrAddManyAsync(IEnumerable<string> keys, Func<IEnumerable<string>, Task<List<KeyValuePair<string, TCacheItem>>>> factory, Func<DistributedCacheEntryOptions>? optionsFactory = null, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default) => throw new NotSupportedException();

    public void Set(string key, TCacheItem value, DistributedCacheEntryOptions? options = null, bool? hideErrors = null, bool considerUow = false)
    {
        SetAsync(key, value, options, hideErrors, considerUow).GetAwaiter().GetResult();
    }

    public Task SetAsync(string key, TCacheItem value, DistributedCacheEntryOptions? options = null, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default)
    {
        _items[key] = value;
        Writes.Add(new CacheWrite<TCacheItem>(key, value, options, considerUow));
        return Task.CompletedTask;
    }

    public void SetMany(IEnumerable<KeyValuePair<string, TCacheItem>> items, DistributedCacheEntryOptions? options = null, bool? hideErrors = null, bool considerUow = false)
    {
        foreach (var item in items)
        {
            Set(item.Key, item.Value, options, hideErrors, considerUow);
        }
    }

    public Task SetManyAsync(IEnumerable<KeyValuePair<string, TCacheItem>> items, DistributedCacheEntryOptions? options = null, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default)
    {
        SetMany(items, options, hideErrors, considerUow);
        return Task.CompletedTask;
    }

    public void Refresh(string key, bool? hideErrors = null)
    {
    }

    public Task RefreshAsync(string key, bool? hideErrors = null, CancellationToken token = default) => Task.CompletedTask;

    public void RefreshMany(IEnumerable<string> keys, bool? hideErrors = null)
    {
    }

    public Task RefreshManyAsync(IEnumerable<string> keys, bool? hideErrors = null, CancellationToken token = default) => Task.CompletedTask;

    public void Remove(string key, bool? hideErrors = null, bool considerUow = false)
    {
        _items.Remove(key);
    }

    public Task RemoveAsync(string key, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default)
    {
        Remove(key, hideErrors, considerUow);
        return Task.CompletedTask;
    }

    public void RemoveMany(IEnumerable<string> keys, bool? hideErrors = null, bool considerUow = false)
    {
        foreach (var key in keys)
        {
            Remove(key, hideErrors, considerUow);
        }
    }

    public Task RemoveManyAsync(IEnumerable<string> keys, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default)
    {
        RemoveMany(keys, hideErrors, considerUow);
        return Task.CompletedTask;
    }
}

internal sealed record CacheWrite<TCacheItem>(string Key, TCacheItem Value, DistributedCacheEntryOptions? Options, bool ConsiderUow);
