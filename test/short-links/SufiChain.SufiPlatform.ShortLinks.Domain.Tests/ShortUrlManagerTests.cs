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

    [Theory]
    [InlineData("https://example.com/docs")]
    [InlineData("http://example.com")]
    [InlineData("  https://example.com/path?q=1  ")]
    public async Task Create_Accepts_An_Absolute_Http_Url(string destination)
    {
        var manager = Manager(featuresEnabled: true);

        var url = await manager.CreateAsync(destination, "CMS", DateTime.UtcNow.AddDays(1));

        url.DestinationUrl.ShouldBe(destination.Trim());
    }

    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData("data:text/html,hi")]
    [InlineData("file:///tmp/secret")]
    [InlineData("/relative")]
    [InlineData("//evil.example")]
    [InlineData("https:///missing-host")]
    public async Task Create_Rejects_A_Non_Http_Destination(string destination)
    {
        var manager = Manager(featuresEnabled: true);

        var error = await Should.ThrowAsync<BusinessException>(() =>
            manager.CreateAsync(destination, "CMS"));
        error.Code.ShouldBe(ShortLinksErrorCodes.InvalidDestinationUrl);
    }

    [Fact]
    public void ChangeDestination_Rejects_A_Stored_Scheme_Change()
    {
        var manager = Manager(featuresEnabled: true);
        var url = new ShortUrl(Guid.NewGuid(), "abc123", "https://example.com", "CMS");

        var error = Should.Throw<BusinessException>(() =>
            manager.ChangeDestination(url, "javascript:alert(1)"));

        error.Code.ShouldBe(ShortLinksErrorCodes.InvalidDestinationUrl);
        url.DestinationUrl.ShouldBe("https://example.com");
    }

    [Fact]
    public void ChangeDestination_Stores_A_Trimmed_Https_Url()
    {
        var manager = Manager(featuresEnabled: true);
        var url = new ShortUrl(Guid.NewGuid(), "abc123", "https://example.com", "CMS");

        manager.ChangeDestination(url, " http://example.com/next ");

        url.DestinationUrl.ShouldBe("http://example.com/next");
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
        error.Code.ShouldBe($"Feature is disabled: {SufiShortLinksFeatures.Enable}");
    }

    private static ShortUrlManager Manager(bool featuresEnabled)
    {
        var repository = Substitute.For<IShortUrlRepository>();
        repository.ShortCodeExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        var features = Substitute.For<IFeatureChecker>();
        features.IsEnabledAsync(Arg.Any<string>()).Returns(featuresEnabled);
        var manager = new ShortUrlManager(
            repository,
            Options.Create(new ShortLinksOptions()),
            Substitute.For<ISettingProvider>(),
            features);
        manager.LazyServiceProvider = LazyGuids();
        return manager;
    }

    private static IAbpLazyServiceProvider LazyGuids()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IGuidGenerator>(SimpleGuidGenerator.Instance);
        return new AbpLazyServiceProvider(services.BuildServiceProvider());
    }
}
