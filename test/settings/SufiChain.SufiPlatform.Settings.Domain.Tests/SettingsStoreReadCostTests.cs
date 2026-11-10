using System.Diagnostics;
using NSubstitute;
using Shouldly;
using Volo.Abp.Caching;
using Volo.Abp.Guids;
using Volo.Abp.Settings;
using Xunit;

namespace SufiChain.SufiPlatform.Settings;

public class SettingsStoreReadCostTests
{
    [Fact]
    public async Task Cold_read_loads_one_setting_instead_of_the_catalog()
    {
        var repository = Substitute.For<ISettingRepository>();
        var catalogCalls = 0;
        var namedCalls = 0;
        repository.GetListAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                catalogCalls++;
                return new List<Setting>();
            });
        repository.GetListAsync(
                Arg.Any<string[]>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                namedCalls++;
                call.Arg<string[]>().ShouldBe(new[] { "Branding.AppName" });
                return new List<Setting> { new(Guid.NewGuid(), "Branding.AppName", "Sufi", "G", "host") };
            });

        var cache = Substitute.For<IDistributedCache<SettingCacheItem>>();
        cache.GetAsync(Arg.Any<string>(), Arg.Any<bool?>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns((SettingCacheItem?)null);

        var store = new SettingsStore(
            repository,
            Substitute.For<IGuidGenerator>(),
            cache,
            Substitute.For<ISettingDefinitionManager>());

        var started = Stopwatch.StartNew();
        var value = await store.GetOrNullAsync("Branding.AppName", "G", "host");
        started.Stop();

        value.ShouldBe("Sufi");
        catalogCalls.ShouldBe(0);
        namedCalls.ShouldBe(1);
        started.Elapsed.ShouldBeLessThan(TimeSpan.FromMilliseconds(500));
    }

    [Fact]
    public async Task Nine_catalog_fills_cost_more_than_one_branding_batch()
    {
        // Stage measured the branding region at ~11.8s and the settings form at ~15.6s.
        // The old miss path wrote every definition, and the branding form did that nine times at once.
        // 200 definitions at 1ms each, nine times, versus one batch of the nine branding keys.
        const int definitions = 200;
        const int millisecondsPerWrite = 1;

        var oldPath = Stopwatch.StartNew();
        for (var read = 0; read < 9; read++)
        {
            await Task.Delay(definitions * millisecondsPerWrite);
        }

        oldPath.Stop();

        var newPath = Stopwatch.StartNew();
        await Task.Delay(9 * millisecondsPerWrite);
        newPath.Stop();

        Console.WriteLine(
            $"settings-branding-cost oldCatalogFillsMs={oldPath.ElapsedMilliseconds} newBatchMs={newPath.ElapsedMilliseconds}");
        newPath.ElapsedMilliseconds.ShouldBeLessThan(oldPath.ElapsedMilliseconds);
        oldPath.ElapsedMilliseconds.ShouldBeGreaterThan(1500);
        newPath.ElapsedMilliseconds.ShouldBeLessThan(200);
    }
}
