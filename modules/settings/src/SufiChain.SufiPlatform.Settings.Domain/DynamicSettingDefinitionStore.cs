using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Volo.Abp;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Settings;
using Volo.Abp.Threading;
using AbpSettingDefinition = Volo.Abp.Settings.SettingDefinition;

namespace SufiChain.SufiPlatform.Settings;

[Dependency(ReplaceServices = true)]
public class DynamicSettingDefinitionStore : IDynamicSettingDefinitionStore, ITransientDependency
{
    protected ISettingDefinitionRecordRepository SettingRepository { get; }
    protected ISettingDefinitionSerializer SettingDefinitionSerializer { get; }
    protected IDynamicSettingDefinitionStoreInMemoryCache StoreCache { get; }
    protected IDistributedCache DistributedCache { get; }
    protected IAbpDistributedLock DistributedLock { get; }
    public SettingsOptions SettingsOptions { get; }
    protected AbpDistributedCacheOptions CacheOptions { get; }

    public DynamicSettingDefinitionStore(
        ISettingDefinitionRecordRepository settingRepository,
        ISettingDefinitionSerializer settingDefinitionSerializer,
        IDynamicSettingDefinitionStoreInMemoryCache storeCache,
        IDistributedCache distributedCache,
        IOptions<AbpDistributedCacheOptions> cacheOptions,
        IOptions<SettingsOptions> settingsOptions,
        IAbpDistributedLock distributedLock)
    {
        SettingRepository = settingRepository;
        SettingDefinitionSerializer = settingDefinitionSerializer;
        StoreCache = storeCache;
        DistributedCache = distributedCache;
        DistributedLock = distributedLock;
        SettingsOptions = settingsOptions.Value;
        CacheOptions = cacheOptions.Value;
    }

    public virtual async Task<AbpSettingDefinition> GetAsync(string name)
    {
        var setting = await GetOrNullAsync(name);
        if (setting == null)
        {
            throw new AbpException("Undefined setting: " + name);
        }

        return setting;
    }

    public virtual async Task<AbpSettingDefinition?> GetOrNullAsync(string name)
    {
        if (!SettingsOptions.IsDynamicSettingStoreEnabled)
        {
            return null;
        }

        using (await StoreCache.SyncSemaphore.LockAsync())
        {
            await EnsureCacheIsUptoDateAsync();
            return StoreCache.GetSettingOrNull(name);
        }
    }

    public virtual async Task<IReadOnlyList<AbpSettingDefinition>> GetAllAsync()
    {
        if (!SettingsOptions.IsDynamicSettingStoreEnabled)
        {
            return Array.Empty<AbpSettingDefinition>();
        }

        using (await StoreCache.SyncSemaphore.LockAsync())
        {
            await EnsureCacheIsUptoDateAsync();
            return StoreCache.GetSettings().ToImmutableList();
        }
    }

    protected virtual async Task EnsureCacheIsUptoDateAsync()
    {
        if (StoreCache.LastCheckTime.HasValue &&
            DateTime.Now.Subtract(StoreCache.LastCheckTime.Value).TotalSeconds < 30)
        {
            return;
        }

        var stampInDistributedCache = await GetOrSetStampInDistributedCache();

        if (stampInDistributedCache == StoreCache.CacheStamp)
        {
            StoreCache.LastCheckTime = DateTime.Now;
            return;
        }

        await UpdateInMemoryStoreCache();

        StoreCache.CacheStamp = stampInDistributedCache;
        StoreCache.LastCheckTime = DateTime.Now;
    }

    protected virtual async Task UpdateInMemoryStoreCache()
    {
        var settingRecords = await SettingRepository.GetListAsync();
        await StoreCache.FillAsync(settingRecords);
    }

    protected virtual async Task<string> GetOrSetStampInDistributedCache()
    {
        var cacheKey = GetCommonStampCacheKey();

        var stampInDistributedCache = await DistributedCache.GetStringAsync(cacheKey);
        if (stampInDistributedCache != null)
        {
            return stampInDistributedCache;
        }

        await using (var commonLockHandle =
                     await DistributedLock.TryAcquireAsync(GetCommonDistributedLockKey(), TimeSpan.FromMinutes(2)))
        {
            if (commonLockHandle == null)
            {
                throw new AbpException(
                    "Could not acquire distributed lock for setting definition common stamp check!"
                );
            }

            stampInDistributedCache = await DistributedCache.GetStringAsync(cacheKey);
            if (stampInDistributedCache != null)
            {
                return stampInDistributedCache;
            }

            stampInDistributedCache = Guid.NewGuid().ToString();

            await DistributedCache.SetStringAsync(
                cacheKey,
                stampInDistributedCache,
                new DistributedCacheEntryOptions
                {
                    SlidingExpiration = TimeSpan.FromDays(30)
                }
            );
        }

        return stampInDistributedCache;
    }

    protected virtual string GetCommonStampCacheKey()
    {
        return $"{CacheOptions.KeyPrefix}_AbpInMemorySettingCacheStamp";
    }

    protected virtual string GetCommonDistributedLockKey()
    {
        return $"{CacheOptions.KeyPrefix}_Common_AbpSettingUpdateLock";
    }
}
