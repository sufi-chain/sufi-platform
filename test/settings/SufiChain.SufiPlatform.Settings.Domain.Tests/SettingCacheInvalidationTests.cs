using NSubstitute;
using Shouldly;
using Volo.Abp.Guids;
using Volo.Abp.Settings;
using Xunit;

namespace SufiChain.SufiPlatform.Settings;

public class SettingCacheInvalidationTests
{
    [Fact]
    public void Setting_value_cache_entries_expire()
    {
        var options = SettingValueCachePolicy.Create();

        options.AbsoluteExpirationRelativeToNow.ShouldBe(SettingValueCachePolicy.AbsoluteExpiration);
        options.AbsoluteExpirationRelativeToNow.ShouldBe(TimeSpan.FromMinutes(10));
    }

    [Fact]
    public async Task Set_writes_the_new_value_immediately_with_an_absolute_expiration()
    {
        var cache = new RecordingDistributedCache<SettingCacheItem>();
        var repository = Substitute.For<ISettingRepository>();
        var store = new SettingsStore(
            repository,
            Substitute.For<IGuidGenerator>(),
            cache,
            Substitute.For<ISettingDefinitionManager>());

        await store.SetAsync("Chat.Ai.Enabled", "true", "G", null!);

        var key = SettingCacheItem.CalculateCacheKey("Chat.Ai.Enabled", "G", "");
        cache.Removes.ShouldContain(remove => remove.Key == key && !remove.ConsiderUow);
        var immediate = cache.Writes.Single(write => write.Key == key && !write.ConsiderUow);
        immediate.Value.Value.ShouldBe("true");
        immediate.Options.ShouldNotBeNull();
        immediate.Options!.AbsoluteExpirationRelativeToNow.ShouldBe(TimeSpan.FromMinutes(10));
        cache.Writes.ShouldContain(write => write.Key == key && write.ConsiderUow);
        (await cache.GetAsync(key)).ShouldNotBeNull().Value.ShouldBe("true");
    }

    [Fact]
    public async Task Changed_setting_is_removed_immediately_for_null_and_empty_provider_keys()
    {
        var cache = new RecordingDistributedCache<SettingCacheItem>();
        var invalidator = new SettingCacheItemInvalidator(cache);
        var setting = new Setting(Guid.NewGuid(), "Chat.Ai.Enabled", "true", "G", null);

        await invalidator.HandleEventAsync(new Volo.Abp.Domain.Entities.Events.EntityChangedEventData<Setting>(setting));

        var nullKey = SettingCacheItem.CalculateCacheKey(setting.Name, setting.ProviderName, null);
        var emptyKey = SettingCacheItem.CalculateCacheKey(setting.Name, setting.ProviderName, "");
        cache.Removes.Select(remove => remove.Key).ShouldContain(nullKey);
        cache.Removes.Select(remove => remove.Key).ShouldContain(emptyKey);
        cache.Removes.ShouldContain(remove => !remove.ConsiderUow);
        cache.Removes.ShouldContain(remove => remove.ConsiderUow);
    }
}
