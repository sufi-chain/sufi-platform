using Shouldly;
using SufiChain.SufiPlatform.SufiCom.Chat.Application.Messages;
using SufiChain.SufiPlatform.SufiCom.Chat.Configuration;
using SufiChain.SufiPlatform.SufiCom.Chat.Settings;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Chat.Messages;

public class ChatOptionsTests
{
    [Fact]
    public void Assistant_turn_timeout_defaults_to_no_limit()
    {
        // 9.4 (sufi-chain#340): 0 means the assistant turn has no wall-clock cutoff.
        new ChatOptions().AssistantTurnTimeoutSeconds.ShouldBe(0);
        ChatSettingDefaults.AiTurnTimeoutSeconds.ShouldBe(0);
    }

    [Fact]
    public async Task A_turn_with_no_deadline_stays_open_and_caller_or_shutdown_cancel_it()
    {
        using var open = ChatAssistantTurnCancellation.Create(CancellationToken.None, CancellationToken.None, 0);
        await Task.Delay(TimeSpan.FromMilliseconds(200));
        open.IsCancellationRequested.ShouldBeFalse();

        using var caller = new CancellationTokenSource();
        caller.Cancel();
        using var callerLinked = ChatAssistantTurnCancellation.Create(CancellationToken.None, caller.Token, 0);
        callerLinked.IsCancellationRequested.ShouldBeTrue();

        using var shutdown = new CancellationTokenSource();
        shutdown.Cancel();
        using var shutdownLinked = ChatAssistantTurnCancellation.Create(shutdown.Token, CancellationToken.None, 0);
        shutdownLinked.IsCancellationRequested.ShouldBeTrue();
    }

    [Fact]
    public async Task An_opted_in_deadline_still_cancels()
    {
        using var limited = ChatAssistantTurnCancellation.Create(CancellationToken.None, CancellationToken.None, 1);
        await Task.Delay(TimeSpan.FromMilliseconds(1500));
        limited.IsCancellationRequested.ShouldBeTrue();
    }
}
