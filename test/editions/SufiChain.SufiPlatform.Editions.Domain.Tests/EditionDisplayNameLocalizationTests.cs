using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;
using Volo.Abp.DependencyInjection;
using Xunit;

namespace SufiChain.SufiPlatform.Editions;

public class EditionDisplayNameLocalizationTests
{
    [Fact]
    public async Task MapToDto_resolves_plan_key_from_registered_resource()
    {
        using var _ = new CultureScope("fa");
        var edition = new Edition(
            Guid.NewGuid(),
            "Scale",
            "Plan:SCALE:DisplayName",
            "SCALE");

        var dto = await MapAsync(
            edition,
            resources: ["Editions", "SufiSaas"],
            translations: new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal)
            {
                ["SufiSaas"] = new(StringComparer.Ordinal)
                {
                    ["Plan:SCALE:DisplayName"] = "مقیاس"
                }
            });

        dto.DisplayName.ShouldBe("Plan:SCALE:DisplayName");
        dto.LocalizedDisplayName.ShouldBe("مقیاس");
    }

    [Fact]
    public async Task MapToDto_keeps_plain_display_name()
    {
        var edition = new Edition(Guid.NewGuid(), "Gold", "Gold", "GOLD");

        var dto = await MapAsync(
            edition,
            resources: ["Editions", "SufiSaas"],
            translations: new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal));

        dto.DisplayName.ShouldBe("Gold");
        dto.LocalizedDisplayName.ShouldBe("Gold");
    }

    [Fact]
    public async Task MapToDto_falls_back_to_unknown_key_without_throwing()
    {
        var stored = "Plan:UNKNOWN:DisplayName";
        var edition = new Edition(Guid.NewGuid(), "Unknown", stored, "UNKNOWN");

        var dto = await MapAsync(
            edition,
            resources: ["Editions", "SufiSaas"],
            translations: new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal)
            {
                ["SufiSaas"] = new(StringComparer.Ordinal)
                {
                    ["Plan:SCALE:DisplayName"] = "Scale"
                }
            });

        dto.DisplayName.ShouldBe(stored);
        dto.LocalizedDisplayName.ShouldBe(stored);
    }

    [Fact]
    public void Resolve_uses_first_resource_that_translates()
    {
        var factory = new FakeLocalizerFactory(new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal)
        {
            ["Editions"] = new(StringComparer.Ordinal),
            ["SufiSaas"] = new(StringComparer.Ordinal)
            {
                ["Plan:SCALE:DisplayName"] = "مقیاس"
            }
        });

        var resolved = EditionDisplayNameLocalization.Resolve(
            factory,
            ["Editions", "SufiSaas"],
            "Plan:SCALE:DisplayName");

        resolved.ShouldBe("مقیاس");
    }

    private static async Task<EditionDto> MapAsync(
        Edition edition,
        IReadOnlyList<string> resources,
        Dictionary<string, Dictionary<string, string>> translations)
    {
        var repository = Substitute.For<IEditionRepository>();
        repository.GetAsync(edition.Id, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(edition);

        var options = Options.Create(new SufiEditionsOptions());
        options.Value.DisplayNameResources.Clear();
        options.Value.DisplayNameResources.AddRange(resources);

        var service = new EditionAppService(
            repository,
            Substitute.For<EditionManager>(Substitute.For<IEditionRepository>()),
            options);

        var services = new ServiceCollection();
        services.AddSingleton<IStringLocalizerFactory>(new FakeLocalizerFactory(translations));
        service.LazyServiceProvider = new AbpLazyServiceProvider(services.BuildServiceProvider());

        return await service.GetAsync(edition.Id);
    }

    private sealed class FakeLocalizerFactory : IStringLocalizerFactory, IAbpStringLocalizerFactory
    {
        private readonly Dictionary<string, Dictionary<string, string>> _byResource;

        public FakeLocalizerFactory(Dictionary<string, Dictionary<string, string>> byResource)
        {
            _byResource = byResource;
        }

        public IStringLocalizer Create(Type resourceSource) =>
            CreateByResourceNameOrNull(resourceSource.Name) ?? new MapLocalizer(new Dictionary<string, string>());

        public IStringLocalizer Create(string baseName, string location) =>
            CreateByResourceNameOrNull(baseName) ?? new MapLocalizer(new Dictionary<string, string>());

        public IStringLocalizer? CreateDefaultOrNull() => null;

        public IStringLocalizer? CreateByResourceNameOrNull(string resourceName) =>
            _byResource.TryGetValue(resourceName, out var map)
                ? new MapLocalizer(map)
                : null;

        public Task<IStringLocalizer?> CreateByResourceNameOrNullAsync(string resourceName) =>
            Task.FromResult(CreateByResourceNameOrNull(resourceName));
    }

    private sealed class MapLocalizer : IStringLocalizer
    {
        private readonly Dictionary<string, string> _map;

        public MapLocalizer(Dictionary<string, string> map) => _map = map;

        public LocalizedString this[string name] =>
            _map.TryGetValue(name, out var value)
                ? new LocalizedString(name, value)
                : new LocalizedString(name, name, resourceNotFound: true);

        public LocalizedString this[string name, params object[] arguments] => this[name];

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) =>
            _map.Select(pair => new LocalizedString(pair.Key, pair.Value));
    }

    private sealed class CultureScope : IDisposable
    {
        private readonly CultureInfo _previousCulture;
        private readonly CultureInfo _previousUiCulture;

        public CultureScope(string culture)
        {
            _previousCulture = CultureInfo.CurrentCulture;
            _previousUiCulture = CultureInfo.CurrentUICulture;
            var info = CultureInfo.GetCultureInfo(culture);
            CultureInfo.CurrentCulture = info;
            CultureInfo.CurrentUICulture = info;
        }

        public void Dispose()
        {
            CultureInfo.CurrentCulture = _previousCulture;
            CultureInfo.CurrentUICulture = _previousUiCulture;
        }
    }
}
