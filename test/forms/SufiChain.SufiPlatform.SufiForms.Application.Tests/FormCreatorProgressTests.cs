using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.SufiCom.Chat.Blazor.Public.Services;
using SufiChain.SufiPlatform.SufiCom.Chat.Realtime;
using Xunit;

namespace SufiChain.SufiPlatform.SufiForms;

public class FormCreatorProgressTests
{
    [Fact]
    public async Task Subscription_joins_before_returning_filters_events_and_rejoins_until_disposed()
    {
        var hub = Substitute.For<IChatHubClientService>();
        var received = new List<ChatAssistantProgressDto>();
        Func<ChatAssistantProgressDto, Task>? receive = null;
        Func<string?, Task>? reconnect = null;
        hub.AssistantProgress += Arg.Do<Func<ChatAssistantProgressDto, Task>>(handler => receive = handler);
        hub.Reconnected += Arg.Do<Func<string?, Task>>(handler => reconnect = handler);
        var joined = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        hub.JoinCopilotTurnAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(joined.Task);
        var starting = ChatTurnProgressSubscription.StartAsync(hub, p =>
        {
            received.Add(p);
            return Task.CompletedTask;
        }, NullLogger.Instance);
        starting.IsCompleted.ShouldBeFalse();
        joined.SetResult(true);
        var subscription = await starting;
        await receive!(new() { TurnId = Guid.NewGuid() });
        await receive(new() { SessionId = Guid.NewGuid() });
        await receive(new() { TurnId = subscription.TurnId });
        received.Count.ShouldBe(1);
        await reconnect!("replacement");
        await hub.Received(2).JoinCopilotTurnAsync(subscription.TurnId, Arg.Any<CancellationToken>());
        await subscription.DisposeAsync();
        await reconnect("late-reconnect");
        await receive(new() { TurnId = subscription.TurnId });
        received.Count.ShouldBe(1);
        await hub.Received(2).JoinCopilotTurnAsync(subscription.TurnId, Arg.Any<CancellationToken>());
        await hub.Received(1).LeaveCopilotTurnAsync(subscription.TurnId, Arg.Any<CancellationToken>());
        await hub.DidNotReceive().DisposeAsync();
    }

    [Fact]
    public async Task Failed_join_detaches_handlers_and_does_not_own_the_shared_connection()
    {
        var hub = Substitute.For<IChatHubClientService>();
        hub.JoinCopilotTurnAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new InvalidOperationException("Unavailable")));
        await Should.ThrowAsync<InvalidOperationException>(() => ChatTurnProgressSubscription.StartAsync(
            hub, _ => Task.CompletedTask, NullLogger.Instance));
        hub.Received(1).AssistantProgress -= Arg.Any<Func<ChatAssistantProgressDto, Task>>();
        hub.Received(1).Reconnected -= Arg.Any<Func<string?, Task>>();
        await hub.DidNotReceive().DisposeAsync();
    }

    [Fact]
    public async Task Disposal_waits_for_inflight_rejoin_then_leaves_even_when_cleanup_fails()
    {
        var hub = Substitute.For<IChatHubClientService>();
        Func<string?, Task>? reconnect = null;
        hub.Reconnected += Arg.Do<Func<string?, Task>>(handler => reconnect = handler);
        var subscription = await ChatTurnProgressSubscription.StartAsync(hub, _ => Task.CompletedTask, NullLogger.Instance);
        var rejoining = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        hub.JoinCopilotTurnAsync(subscription.TurnId, Arg.Any<CancellationToken>()).Returns(rejoining.Task);
        hub.LeaveCopilotTurnAsync(subscription.TurnId, Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new InvalidOperationException("Connection closed")));
        var reconnecting = reconnect!("replacement");
        var disposing = subscription.DisposeAsync().AsTask();
        disposing.IsCompleted.ShouldBeFalse();
        rejoining.SetResult(true);
        await Task.WhenAll(reconnecting, disposing);
        await reconnect("late");
        await hub.Received(2).JoinCopilotTurnAsync(subscription.TurnId, Arg.Any<CancellationToken>());
        await hub.Received(1).LeaveCopilotTurnAsync(subscription.TurnId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Local_messenger_accepts_only_its_active_turn_and_merges_tool_results()
    {
        var state = new ChatMessengerState();
        var other = new ChatMessengerState();
        var turn = Guid.NewGuid();
        state.BeginProgressTurn(turn);
        other.BeginProgressTurn(Guid.NewGuid());
        var progress = new ChatAssistantProgressDto
        {
            TurnId = turn, Stage = "calling_tool", Status = "started", ToolName = "forms.get_designer_catalog"
        };
        state.ApplyAssistantProgress(progress);
        other.ApplyAssistantProgress(progress);
        other.AiProgressSteps.ShouldBeEmpty();
        progress.Stage = "tool_result";
        progress.Status = "succeeded";
        state.ApplyAssistantProgress(progress);
        state.AiProgressSteps.Count.ShouldBe(1);
        state.AiProgressSteps[0].Status.ShouldBe("succeeded");
        state.EndProgressTurn();
        progress.Status = "failed";
        state.ApplyAssistantProgress(progress);
        state.AiProgressSteps[0].Status.ShouldBe("succeeded");
        state.BeginProgressTurn(Guid.NewGuid());
        state.ApplyAssistantProgress(progress);
        state.AiProgressSteps.ShouldBeEmpty();

        state.Reset();
        state.SelectedSessionId = Guid.NewGuid();
        state.ApplyAssistantProgress(new() { SessionId = state.SelectedSessionId.Value, Stage = "preparing" });
        state.AiProgressSteps.Count.ShouldBe(1);
    }
}
