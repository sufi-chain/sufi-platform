using Xunit;

namespace SufiChain.SufiPlatform.SufiAI;

public class LlmChatIdleTests
{
    [Fact]
    public async Task Silence_cancels_and_a_live_stream_does_not()
    {
        using (var silent = new LlmChatIdle(CancellationToken.None, TimeSpan.FromMilliseconds(150)))
        {
            await Task.Delay(TimeSpan.FromMilliseconds(400));
            Assert.True(silent.Token.IsCancellationRequested);
        }

        using var live = new LlmChatIdle(CancellationToken.None, TimeSpan.FromMilliseconds(200));
        var started = DateTime.UtcNow;
        while (DateTime.UtcNow - started < TimeSpan.FromMilliseconds(700))
        {
            await Task.Delay(TimeSpan.FromMilliseconds(40));
            Assert.False(live.Token.IsCancellationRequested);
            live.Arm();
        }
    }

    [Fact]
    public async Task Zero_idle_does_not_cancel_and_the_caller_still_can()
    {
        using var open = new LlmChatIdle(CancellationToken.None, Timeout.InfiniteTimeSpan);
        await Task.Delay(TimeSpan.FromMilliseconds(100));
        Assert.False(open.Token.IsCancellationRequested);

        using var caller = new CancellationTokenSource();
        using var linked = new LlmChatIdle(caller.Token, Timeout.InfiniteTimeSpan);
        caller.Cancel();
        Assert.True(linked.Token.IsCancellationRequested);
    }
}
