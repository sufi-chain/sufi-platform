using Shouldly;
using SufiChain.SufiPlatform.Dashboard.Dashboard;
using Xunit;

namespace SufiChain.SufiPlatform.Dashboard.Application.Tests;

public class DashboardShortcutClientLoadTests
{
    [Fact]
    public async Task Loading_empty_list_returns_empty()
    {
        var result = await DashboardShortcutClientLoad.LoadOrEmptyAsync(
            _ => Task.FromResult<List<DashboardShortcutDto>?>([]),
            CancellationToken.None);

        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task Loading_error_returns_empty()
    {
        var result = await DashboardShortcutClientLoad.LoadOrEmptyAsync(
            _ => throw new InvalidOperationException("menu stall"),
            CancellationToken.None);

        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task Loading_timeout_returns_empty_without_hanging()
    {
        var result = await DashboardShortcutClientLoad.LoadOrEmptyAsync(
            async ct =>
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, ct);
                return [];
            },
            CancellationToken.None,
            timeout: TimeSpan.FromMilliseconds(50));

        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task Parent_cancellation_rethrows()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Should.ThrowAsync<OperationCanceledException>(() =>
            DashboardShortcutClientLoad.LoadOrEmptyAsync(
                async ct =>
                {
                    ct.ThrowIfCancellationRequested();
                    await Task.Yield();
                    return [];
                },
                cts.Token));
    }
}
