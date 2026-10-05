using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.Settings;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Features;
using Volo.Abp.MultiTenancy;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;

public class HooshvareUserShortcutAppServiceTests
{
    [Fact]
    public async Task GetMyAsync_returns_empty_when_the_hooshvare_is_missing()
    {
        var service = CreateService(definition: null, featureEnabled: true);

        var shortcuts = await service.GetMyAsync("SufiCMS:SiteBuilder");

        shortcuts.HooshvareKey.ShouldBe("SufiCMS:SiteBuilder");
        shortcuts.IsCustomized.ShouldBeFalse();
        shortcuts.Prompts.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetMyAsync_returns_empty_when_the_hooshvare_is_disabled()
    {
        var definition = CreateDefinition("SufiCMS:SiteBuilder", enabled: false);
        var service = CreateService(definition, featureEnabled: true);

        var shortcuts = await service.GetMyAsync(definition.Key!);

        shortcuts.Prompts.ShouldBeEmpty();
        shortcuts.IsCustomized.ShouldBeFalse();
    }

    [Fact]
    public async Task GetMyAsync_returns_empty_when_the_hooshvare_is_hidden()
    {
        var definition = CreateDefinition(PlatformHooshvareKeys.HooshvareCoach, enabled: true);
        var service = CreateService(definition, featureEnabled: true);

        var shortcuts = await service.GetMyAsync(definition.Key!);

        shortcuts.Prompts.ShouldBeEmpty();
    }

    [Fact]
    public async Task ReplaceAsync_still_rejects_an_unavailable_hooshvare()
    {
        var service = CreateService(definition: null, featureEnabled: true);

        var exception = await Should.ThrowAsync<BusinessException>(() => service.ReplaceAsync(
            new ReplaceHooshvareUserShortcutsInput
            {
                HooshvareKey = "SufiCMS:SiteBuilder",
                Prompts = ["Hello"]
            }));

        exception.Code.ShouldBe(AIHooshvareErrorCodes.HooshvareNotFound);
    }

    private static HooshvareUserShortcutAppService CreateService(HooshvareDefinition? definition, bool featureEnabled)
    {
        var repository = Substitute.For<IHooshvareDefinitionRepository>();
        repository.FindByKeyAsync(Arg.Any<string>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns(definition);

        var features = Substitute.For<IFeatureChecker>();
        features.IsEnabledAsync(Arg.Any<string>()).Returns(featureEnabled);

        var tenant = Substitute.For<ICurrentTenant>();
        tenant.Id.Returns((Guid?)null);

        var services = new ServiceCollection();
        services.AddSingleton(tenant);

        var service = new HooshvareUserShortcutAppService(
            repository,
            new HooshvareAvailabilityChecker(features),
            Substitute.For<ISettingsStore>(),
            Substitute.For<IStringLocalizerFactory>());
        service.LazyServiceProvider = new AbpLazyServiceProvider(services.BuildServiceProvider());
        return service;
    }

    private static HooshvareDefinition CreateDefinition(string key, bool enabled)
    {
        var definition = new HooshvareDefinition(
            Guid.NewGuid(),
            null,
            "SufiCMS",
            "Site builder",
            HooshvareKind.Assistant,
            "SiteBuilder",
            Guid.NewGuid(),
            "prompt",
            persistChatSession: false,
            key: key,
            defaultEnabled: enabled);

        if (!enabled)
        {
            definition.Disable();
        }

        return definition;
    }
}
