using Shouldly;
using SufiChain.SufiPlatform.AspNetCore.Authentication.Server.Controllers;
using Xunit;

namespace SufiChain.SufiPlatform.Account;

public class LogoutSessionTests
{
    [Fact]
    public async Task Logout_WhenSecurityLogFails_StillRedirectsAndClearsCookies()
    {
        var signedOut = 0;
        await LogoutSession.RunAsync(
            () => throw new InvalidOperationException("log"),
            () =>
            {
                signedOut++;
                return Task.CompletedTask;
            },
            [],
            _ => { });

        signedOut.ShouldBe(1);
    }

    [Fact]
    public async Task Logout_WhenSignOutHandlerThrows_StillRedirects()
    {
        var extras = 0;
        await LogoutSession.RunAsync(
            () => Task.CompletedTask,
            () => throw new InvalidOperationException("cookie"),
            [() =>
            {
                extras++;
                return Task.CompletedTask;
            }],
            _ => { });

        extras.ShouldBe(1);
    }

    [Fact]
    public async Task Logout_Anonymous_Redirects()
    {
        var signedOut = 0;
        await LogoutSession.RunAsync(
            () => Task.CompletedTask,
            () =>
            {
                signedOut++;
                return Task.CompletedTask;
            },
            [],
            _ => { });

        signedOut.ShouldBe(1);
    }

    [Fact]
    public async Task Logout_TwiceConcurrently_BothRedirect()
    {
        var signedOut = 0;
        var runs = Enumerable.Range(0, 2).Select(_ => LogoutSession.RunAsync(
            () => Task.CompletedTask,
            () =>
            {
                Interlocked.Increment(ref signedOut);
                return Task.CompletedTask;
            },
            [],
            _ => { }));

        await Task.WhenAll(runs);
        signedOut.ShouldBe(2);
    }
}
