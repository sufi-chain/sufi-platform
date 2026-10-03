using Microsoft.Extensions.Caching.Distributed;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.Identity;
using SufiChain.SufiPlatform.Identity.Settings;
using Volo.Abp;
using Volo.Abp.Caching;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Settings;
using Volo.Abp.Timing;
using Xunit;

namespace SufiChain.SufiPlatform.Account.Otp;

public class OtpResendCooldownTests
{
    private static readonly DateTime Start = new(2026, 10, 3, 12, 0, 0, DateTimeKind.Utc);

    [Theory]
    [InlineData(1, 120)]
    [InlineData(5, 120)]
    [InlineData(6, 240)]
    [InlineData(7, 480)]
    [InlineData(15, 86400)]
    [InlineData(500, 86400)]
    public void Options_Should_Double_The_Wait_After_The_Backoff_Threshold(int sendCount, int expected)
    {
        new OtpResendCooldownOptions().GetCooldownSeconds(sendCount).ShouldBe(expected);
    }

    [Fact]
    public void Options_Should_Keep_A_Flat_Wait_Without_A_Multiplier()
    {
        new OtpResendCooldownOptions { BackoffMultiplier = 1 }.GetCooldownSeconds(20).ShouldBe(120);
    }

    [Fact]
    public void Options_Should_Be_Disabled_With_A_Zero_Cooldown()
    {
        var options = new OtpResendCooldownOptions { CooldownSeconds = 0 };

        options.IsEnabled.ShouldBeFalse();
        options.GetCooldownSeconds(10).ShouldBe(0);
    }

    [Fact]
    public async Task Store_Should_Allow_The_First_Request_And_Keep_The_Counter_For_The_Reset_Window()
    {
        var fixture = new StoreFixture();

        var result = await fixture.Store.TryAcquireAsync("+989121234567", new OtpResendCooldownOptions());

        result.IsAllowed.ShouldBeTrue();
        result.WaitSeconds.ShouldBe(120);
        var item = fixture.Items.Values.ShouldHaveSingleItem();
        item.SendCount.ShouldBe(1);
        item.NextAllowedAt.ShouldBe(Start.AddSeconds(120));
        fixture.LastEntryOptions!.AbsoluteExpirationRelativeToNow.ShouldBe(TimeSpan.FromHours(24));
    }

    [Fact]
    public async Task Store_Should_Reject_During_The_Cooldown_Without_Extending_It()
    {
        var fixture = new StoreFixture();
        var options = new OtpResendCooldownOptions();
        await fixture.Store.TryAcquireAsync("+989121234567", options);

        fixture.Now = Start.AddSeconds(30);
        var result = await fixture.Store.TryAcquireAsync("+989121234567", options);

        result.IsAllowed.ShouldBeFalse();
        result.WaitSeconds.ShouldBe(90);
        var item = fixture.Items.Values.ShouldHaveSingleItem();
        item.SendCount.ShouldBe(1);
        item.NextAllowedAt.ShouldBe(Start.AddSeconds(120));
    }

    [Fact]
    public async Task Store_Should_Double_The_Wait_From_The_Sixth_Accepted_Request()
    {
        var fixture = new StoreFixture();
        var options = new OtpResendCooldownOptions();
        var waits = new List<int>();

        for (var i = 0; i < 7; i++)
        {
            var result = await fixture.Store.TryAcquireAsync("+989121234567", options);
            result.IsAllowed.ShouldBeTrue();
            waits.Add(result.WaitSeconds);
            fixture.Now = fixture.Now.AddSeconds(result.WaitSeconds);
        }

        waits.ShouldBe(new[] { 120, 120, 120, 120, 120, 240, 480 });
    }

    [Fact]
    public async Task Store_Should_Share_One_Cooldown_Across_Phone_Formats()
    {
        var fixture = new StoreFixture();
        var options = new OtpResendCooldownOptions();

        (await fixture.Store.TryAcquireAsync("+989121234567", options)).IsAllowed.ShouldBeTrue();
        (await fixture.Store.TryAcquireAsync("09121234567", options)).IsAllowed.ShouldBeFalse();
        (await fixture.Store.TryAcquireAsync("۰۹۱۲۱۲۳۴۵۶۷", options)).IsAllowed.ShouldBeFalse();
        (await fixture.Store.TryAcquireAsync("+989121234568", options)).IsAllowed.ShouldBeTrue();
    }

    [Fact]
    public async Task Store_Should_Reject_When_Another_Request_Holds_The_Lock()
    {
        var fixture = new StoreFixture { LockAvailable = false };

        var result = await fixture.Store.TryAcquireAsync("+989121234567", new OtpResendCooldownOptions());

        result.IsAllowed.ShouldBeFalse();
        result.WaitSeconds.ShouldBe(120);
        fixture.Items.ShouldBeEmpty();
    }

    [Fact]
    public async Task Store_Should_Not_Touch_The_Cache_When_Disabled()
    {
        var fixture = new StoreFixture();

        var result = await fixture.Store.TryAcquireAsync(
            "+989121234567",
            new OtpResendCooldownOptions { CooldownSeconds = 0 });

        result.IsAllowed.ShouldBeTrue();
        result.WaitSeconds.ShouldBe(0);
        fixture.Items.ShouldBeEmpty();
    }

    [Theory]
    [InlineData("+989121234567", "989121234567")]
    [InlineData("0098 912 123 4567", "989121234567")]
    [InlineData("+1 (415) 555-0100", "14155550100")]
    public void NormalizePhone_Should_Produce_A_Stable_Key(string phone, string expected)
    {
        OtpResendCooldownStore.NormalizePhone(phone).ShouldBe(expected);
    }

    [Fact]
    public async Task Guard_Should_Skip_Email()
    {
        var store = Substitute.For<IOtpResendCooldownStore>();
        var guard = new OtpResendCooldownGuard(store, NewSettingProvider());

        (await guard.EnsureAllowedAsync(VerificationDeliveryChannel.Email, "user@example.com")).ShouldBe(0);

        await store.DidNotReceiveWithAnyArgs().TryAcquireAsync(default!, default!, default);
    }

    [Fact]
    public async Task Guard_Should_Return_The_Next_Wait_When_Allowed()
    {
        var store = Substitute.For<IOtpResendCooldownStore>();
        store.TryAcquireAsync(Arg.Any<string>(), Arg.Any<OtpResendCooldownOptions>(), Arg.Any<CancellationToken>())
            .Returns(OtpResendCooldownResult.Allowed(120));
        var guard = new OtpResendCooldownGuard(store, NewSettingProvider());

        (await guard.EnsureAllowedAsync(VerificationDeliveryChannel.Sms, "+989121234567")).ShouldBe(120);

        await store.Received(1).TryAcquireAsync(
            "+989121234567",
            Arg.Is<OtpResendCooldownOptions>(o => o.CooldownSeconds == 120 && o.BackoffAfterAttempts == 5),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Guard_Should_Throw_With_The_Remaining_Seconds_When_Rejected()
    {
        var store = Substitute.For<IOtpResendCooldownStore>();
        store.TryAcquireAsync(Arg.Any<string>(), Arg.Any<OtpResendCooldownOptions>(), Arg.Any<CancellationToken>())
            .Returns(OtpResendCooldownResult.Rejected(90));
        var guard = new OtpResendCooldownGuard(store, NewSettingProvider());

        var exception = await Should.ThrowAsync<BusinessException>(
            () => guard.EnsureAllowedAsync(VerificationDeliveryChannel.Voice, "+989121234567"));

        exception.Code.ShouldBe(IdentitySecurityErrorCodes.OtpResendCooldown);
        exception.Data[OtpResendCooldownGuard.SecondsDataKey].ShouldBe(90);
    }

    private static ISettingProvider NewSettingProvider()
    {
        var values = new Dictionary<string, string>
        {
            [IdentitySettingNames.Otp.ResendCooldownSeconds] = "120",
            [IdentitySettingNames.Otp.ResendBackoffAfterAttempts] = "5",
            [IdentitySettingNames.Otp.ResendBackoffMultiplier] = "2",
            [IdentitySettingNames.Otp.ResendMaxCooldownSeconds] = "86400",
            [IdentitySettingNames.Otp.ResendCounterResetHours] = "24"
        };

        var settingProvider = Substitute.For<ISettingProvider>();
        settingProvider.GetOrNullAsync(Arg.Any<string>())
            .Returns(call => values.GetValueOrDefault(call.ArgAt<string>(0)));
        return settingProvider;
    }

    private sealed class StoreFixture
    {
        public StoreFixture()
        {
            var cache = Substitute.For<IDistributedCache<OtpResendCooldownCacheItem>>();
            cache.GetAsync(Arg.Any<string>(), Arg.Any<bool?>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
                .Returns(call => Items.GetValueOrDefault(call.ArgAt<string>(0)));
            cache.SetAsync(
                    Arg.Any<string>(),
                    Arg.Any<OtpResendCooldownCacheItem>(),
                    Arg.Any<DistributedCacheEntryOptions?>(),
                    Arg.Any<bool?>(),
                    Arg.Any<bool>(),
                    Arg.Any<CancellationToken>())
                .Returns(call =>
                {
                    Items[call.ArgAt<string>(0)] = call.ArgAt<OtpResendCooldownCacheItem>(1);
                    LastEntryOptions = call.ArgAt<DistributedCacheEntryOptions?>(2);
                    return Task.CompletedTask;
                });

            var distributedLock = Substitute.For<IAbpDistributedLock>();
            distributedLock.TryAcquireAsync(Arg.Any<string>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
                .Returns(_ => LockAvailable ? Substitute.For<IAbpDistributedLockHandle>() : null);

            var clock = Substitute.For<IClock>();
            clock.Now.Returns(_ => Now);

            Store = new OtpResendCooldownStore(cache, distributedLock, clock);
        }

        public OtpResendCooldownStore Store { get; }

        public Dictionary<string, OtpResendCooldownCacheItem> Items { get; } = new();

        public DistributedCacheEntryOptions? LastEntryOptions { get; private set; }

        public DateTime Now { get; set; } = Start;

        public bool LockAvailable { get; set; } = true;
    }
}
