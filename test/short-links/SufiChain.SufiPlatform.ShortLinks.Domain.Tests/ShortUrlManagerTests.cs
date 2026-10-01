using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.Features;
using SufiChain.SufiPlatform.ShortLinks.Features;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Volo.Abp.Settings;
using Xunit;

namespace SufiChain.SufiPlatform.ShortLinks;

public class ShortUrlManagerTests
{
    [Fact]
    public async Task Create_Should_Return_An_Active_Short_Url()
    {
        var repository = Substitute.For<IShortUrlRepository>();
        repository.ShortCodeExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        var features = Substitute.For<IFeatureChecker>();
        features.IsEnabledAsync(Arg.Any<string>()).Returns(true);
        var manager = new ShortUrlManager(
            repository,
            Options.Create(new ShortLinksOptions()),
            Substitute.For<ISettingProvider>(),
            features);
        manager.LazyServiceProvider = LazyGuids();

        var url = await manager.CreateAsync("https://example.com/docs", "CMS", DateTime.UtcNow.AddDays(1));

        url.DestinationUrl.ShouldBe("https://example.com/docs");
        url.IsActive.ShouldBeTrue();
        url.ShortCode.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Create_Should_Reject_When_Short_Links_Are_Disabled()
    {
        var features = Substitute.For<IFeatureChecker>();
        features.IsEnabledAsync(SufiShortLinksFeatures.Enable).Returns(false);
        var manager = new ShortUrlManager(
            Substitute.For<IShortUrlRepository>(),
            Options.Create(new ShortLinksOptions()),
            Substitute.For<ISettingProvider>(),
            features);

        var error = await Should.ThrowAsync<BusinessException>(() =>
            manager.CreateAsync("https://example.com", "CMS"));
        error.Message.ShouldContain(SufiShortLinksFeatures.Enable);
    }

    private static IAbpLazyServiceProvider LazyGuids()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IGuidGenerator>(SimpleGuidGenerator.Instance);
        return new AbpLazyServiceProvider(services.BuildServiceProvider());
    }
}
