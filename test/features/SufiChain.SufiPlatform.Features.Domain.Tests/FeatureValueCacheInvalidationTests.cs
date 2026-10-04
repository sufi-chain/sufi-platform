using NSubstitute;
using Shouldly;
using Volo.Abp.Guids;
using Xunit;

namespace SufiChain.SufiPlatform.Features;

public class FeatureValueCacheInvalidationTests
{
    [Fact]
    public void Feature_value_cache_entries_expire()
    {
        var options = FeatureValueCachePolicy.Create();

        options.AbsoluteExpirationRelativeToNow.ShouldBe(FeatureValueCachePolicy.AbsoluteExpiration);
        options.AbsoluteExpirationRelativeToNow.ShouldBe(TimeSpan.FromMinutes(10));
    }

    [Fact]
    public async Task Set_writes_the_new_value_immediately_with_an_absolute_expiration()
    {
        var cache = new RecordingDistributedCache<FeatureValueCacheItem>();
        var repository = Substitute.For<IFeatureValueRepository>();
        var store = new FeaturesStore(
            repository,
            Substitute.For<IGuidGenerator>(),
            cache,
            Substitute.For<IFeatureDefinitionManager>());

        await store.SetAsync("Chat.Ai.Enable", "true", "G", null!);

        var key = FeatureValueCacheItem.CalculateCacheKey("Chat.Ai.Enable", "G", null);
        cache.Removes.ShouldContain(remove => remove.Key == key && !remove.ConsiderUow);
        var immediate = cache.Writes.Single(write => write.Key == key && !write.ConsiderUow);
        immediate.Value.Value.ShouldBe("true");
        immediate.Options.ShouldNotBeNull();
        immediate.Options!.AbsoluteExpirationRelativeToNow.ShouldBe(TimeSpan.FromMinutes(10));
        (await cache.GetAsync(key)).ShouldNotBeNull().Value.ShouldBe("true");
    }

    [Fact]
    public async Task Changed_feature_is_removed_immediately_for_null_and_empty_provider_keys()
    {
        var cache = new RecordingDistributedCache<FeatureValueCacheItem>();
        var invalidator = new FeatureValueCacheItemInvalidator(cache);
        var feature = new FeatureValue(Guid.NewGuid(), "Chat.Ai.Enable", "true", "G", null);

        await invalidator.HandleEventAsync(new Volo.Abp.Domain.Entities.Events.EntityChangedEventData<FeatureValue>(feature));

        var nullKey = FeatureValueCacheItem.CalculateCacheKey(feature.Name, feature.ProviderName, null);
        var emptyKey = FeatureValueCacheItem.CalculateCacheKey(feature.Name, feature.ProviderName, "");
        cache.Removes.Select(remove => remove.Key).ShouldContain(nullKey);
        cache.Removes.Select(remove => remove.Key).ShouldContain(emptyKey);
        cache.Removes.ShouldContain(remove => !remove.ConsiderUow);
        cache.Removes.ShouldContain(remove => remove.ConsiderUow);
    }
}
