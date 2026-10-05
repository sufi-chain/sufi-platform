using System.Reflection;
using Microsoft.Extensions.Caching.Distributed;
using NSubstitute;
using Shouldly;
using Volo.Abp.Caching;
using Xunit;

namespace SufiChain.SufiPlatform.Account;

public class PhoneConfirmationSessionStoreTests
{
    [Fact]
    public async Task Create_Find_And_Revoke_Round_Trip()
    {
        var fixture = new StoreFixture();
        var userId = Guid.NewGuid();

        var token = await fixture.Store.CreateAsync(userId);

        token.Length.ShouldBe(64);
        fixture.LastEntryOptions!.AbsoluteExpirationRelativeToNow.ShouldBe(TimeSpan.FromMinutes(PhoneConfirmationSessionStore.LifetimeMinutes));
        (await fixture.Store.FindUserIdAsync(token)).ShouldBe(userId);
        (await fixture.Store.FindUserIdAsync(" " + token + " ")).ShouldBe(userId);

        await fixture.Store.RevokeAsync(token);

        (await fixture.Store.FindUserIdAsync(token)).ShouldBeNull();
    }

    [Fact]
    public async Task Unknown_Or_Blank_Token_Returns_Null()
    {
        var fixture = new StoreFixture();

        (await fixture.Store.FindUserIdAsync("missing")).ShouldBeNull();
        (await fixture.Store.FindUserIdAsync("  ")).ShouldBeNull();
        (await fixture.Store.FindUserIdAsync(null)).ShouldBeNull();
    }

    [Fact]
    public async Task Empty_User_Id_Is_Treated_As_Missing()
    {
        var fixture = new StoreFixture();
        fixture.Items["Account:PhoneConfirmSession:empty"] = new PhoneConfirmationSessionCacheItem();

        (await fixture.Store.FindUserIdAsync("empty")).ShouldBeNull();
    }

    [Fact]
    public void Phone_Confirmation_Dtos_Do_Not_Accept_A_Raw_User_Id()
    {
        typeof(SendPhoneConfirmationCodeDto).GetProperty("UserId").ShouldBeNull();
        typeof(ConfirmPhoneNumberDto).GetProperty("UserId").ShouldBeNull();
        typeof(SendPhoneConfirmationCodeDto).GetProperty(nameof(SendPhoneConfirmationCodeDto.SessionToken)).ShouldNotBeNull();
        typeof(ConfirmPhoneNumberDto).GetProperty(nameof(ConfirmPhoneNumberDto.SessionToken)).ShouldNotBeNull();
        typeof(IAccountAppService).GetMethod(nameof(IAccountAppService.GetPhoneConfirmationStateAsync))!
            .GetParameters()
            .Single()
            .ParameterType.ShouldBe(typeof(string));
    }

    private sealed class StoreFixture
    {
        public StoreFixture()
        {
            var cache = Substitute.For<IDistributedCache<PhoneConfirmationSessionCacheItem>>();
            cache.GetAsync(Arg.Any<string>(), Arg.Any<bool?>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
                .Returns(call => Items.GetValueOrDefault(call.ArgAt<string>(0)));
            cache.SetAsync(
                    Arg.Any<string>(),
                    Arg.Any<PhoneConfirmationSessionCacheItem>(),
                    Arg.Any<DistributedCacheEntryOptions?>(),
                    Arg.Any<bool?>(),
                    Arg.Any<bool>(),
                    Arg.Any<CancellationToken>())
                .Returns(call =>
                {
                    Items[call.ArgAt<string>(0)] = call.ArgAt<PhoneConfirmationSessionCacheItem>(1);
                    LastEntryOptions = call.ArgAt<DistributedCacheEntryOptions?>(2);
                    return Task.CompletedTask;
                });
            cache.RemoveAsync(Arg.Any<string>(), Arg.Any<bool?>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
                .Returns(call =>
                {
                    Items.Remove(call.ArgAt<string>(0));
                    return Task.CompletedTask;
                });

            Store = new PhoneConfirmationSessionStore(cache);
        }

        public PhoneConfirmationSessionStore Store { get; }

        public Dictionary<string, PhoneConfirmationSessionCacheItem> Items { get; } = new();

        public DistributedCacheEntryOptions? LastEntryOptions { get; private set; }
    }
}
