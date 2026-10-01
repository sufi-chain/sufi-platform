using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.Account.Otp;
using Volo.Abp.Caching;
using Xunit;

namespace SufiChain.SufiPlatform.Account.Otp;

public class AccountOtpCodeStoreTests
{
    [Fact]
    public async Task TryIncrementRateLimit_Should_Allow_The_First_Attempt()
    {
        var rateLimit = Substitute.For<IDistributedCache<OtpRateLimitCacheItem>>();
        rateLimit.GetAsync(Arg.Any<string>(), Arg.Any<bool?>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns((OtpRateLimitCacheItem?)null);
        var store = NewStore(rateLimit);

        var allowed = await store.TryIncrementRateLimitAsync(
            VerificationPurpose.OtpLogin, VerificationDeliveryChannel.Email, "user@example.com", 3);

        allowed.ShouldBeTrue();
    }

    [Fact]
    public async Task TryIncrementRateLimit_Should_Reject_When_The_Hourly_Cap_Is_Reached()
    {
        var rateLimit = Substitute.For<IDistributedCache<OtpRateLimitCacheItem>>();
        rateLimit.GetAsync(Arg.Any<string>(), Arg.Any<bool?>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(new OtpRateLimitCacheItem { Count = 3 });
        var store = NewStore(rateLimit);

        var allowed = await store.TryIncrementRateLimitAsync(
            VerificationPurpose.OtpLogin, VerificationDeliveryChannel.Email, "user@example.com", 3);

        allowed.ShouldBeFalse();
    }

    [Fact]
    public async Task ConsumeRegistrationToken_Should_Return_Null_On_Replay()
    {
        var tokens = Substitute.For<IDistributedCache<string>>();
        tokens.GetAsync(Arg.Any<string>(), Arg.Any<bool?>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns("user@example.com", (string?)null);
        var store = new OtpCodeStore(
            Substitute.For<IDistributedCache<OtpCacheItem>>(),
            Substitute.For<IDistributedCache<OtpRateLimitCacheItem>>(),
            tokens);

        (await store.ConsumeRegistrationTokenAsync("token-1")).ShouldBe("user@example.com");
        (await store.ConsumeRegistrationTokenAsync("token-1")).ShouldBeNull();
    }

    private static OtpCodeStore NewStore(IDistributedCache<OtpRateLimitCacheItem> rateLimit) =>
        new(Substitute.For<IDistributedCache<OtpCacheItem>>(), rateLimit, Substitute.For<IDistributedCache<string>>());
}
