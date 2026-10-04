using Shouldly;
using SufiChain.SufiPlatform.SufiCom.Chat.Application.Messages;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Chat.Messages;

public class ChatAssistantSessionLanesTests
{
    [Fact]
    public async Task A_slow_session_does_not_block_another_session_and_keeps_its_own_order()
    {
        var lanes = new ChatAssistantSessionLanes();
        var sessionA = Guid.NewGuid();
        var sessionB = Guid.NewGuid();
        var firstStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseFirst = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var secondDone = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var otherDone = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        lanes.Enqueue(sessionA, async () =>
        {
            firstStarted.SetResult();
            await releaseFirst.Task;
        });
        lanes.Enqueue(sessionA, () =>
        {
            secondDone.SetResult();
            return Task.CompletedTask;
        });
        lanes.Enqueue(sessionB, () =>
        {
            otherDone.SetResult();
            return Task.CompletedTask;
        });

        await firstStarted.Task.WaitAsync(TimeSpan.FromSeconds(2));
        await otherDone.Task.WaitAsync(TimeSpan.FromSeconds(2));
        secondDone.Task.IsCompleted.ShouldBeFalse();

        releaseFirst.SetResult();
        await secondDone.Task.WaitAsync(TimeSpan.FromSeconds(2));
    }

    [Fact]
    public async Task A_failing_turn_does_not_stop_later_turns_and_idle_lanes_are_removed()
    {
        var lanes = new ChatAssistantSessionLanes();
        var sessionId = Guid.NewGuid();
        var order = new List<int>();
        var done = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        lanes.Enqueue(sessionId, () =>
        {
            order.Add(1);
            throw new InvalidOperationException("boom");
        });
        lanes.Enqueue(sessionId, () =>
        {
            order.Add(2);
            return Task.CompletedTask;
        });
        lanes.Enqueue(sessionId, () =>
        {
            order.Add(3);
            done.SetResult();
            return Task.CompletedTask;
        });

        await done.Task.WaitAsync(TimeSpan.FromSeconds(2));
        order.ShouldBe(new[] { 1, 2, 3 });

        var deadline = DateTime.UtcNow.AddSeconds(2);
        while (lanes.ActiveLaneCount != 0 && DateTime.UtcNow < deadline)
        {
            await Task.Delay(10);
        }

        lanes.ActiveLaneCount.ShouldBe(0);
    }

    [Fact]
    public async Task A_new_turn_after_the_lane_went_idle_still_runs()
    {
        var lanes = new ChatAssistantSessionLanes();
        var sessionId = Guid.NewGuid();
        var first = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var second = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        lanes.Enqueue(sessionId, () => { first.SetResult(); return Task.CompletedTask; });
        await first.Task.WaitAsync(TimeSpan.FromSeconds(2));

        var deadline = DateTime.UtcNow.AddSeconds(2);
        while (lanes.ActiveLaneCount != 0 && DateTime.UtcNow < deadline)
        {
            await Task.Delay(10);
        }

        lanes.Enqueue(sessionId, () => { second.SetResult(); return Task.CompletedTask; });
        await second.Task.WaitAsync(TimeSpan.FromSeconds(2));
    }

    [Fact]
    public async Task A_turn_enqueued_while_another_is_running_on_the_same_session_runs_after_it()
    {
        var lanes = new ChatAssistantSessionLanes();
        var sessionId = Guid.NewGuid();
        var order = new List<int>();
        var overlap = 0;
        var maxOverlap = 0;
        var firstStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseFirst = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var done = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        lanes.Enqueue(sessionId, async () =>
        {
            var now = Interlocked.Increment(ref overlap);
            maxOverlap = Math.Max(maxOverlap, now);
            order.Add(1);
            firstStarted.SetResult();
            await releaseFirst.Task;
            Interlocked.Decrement(ref overlap);
        });

        await firstStarted.Task.WaitAsync(TimeSpan.FromSeconds(2));

        lanes.Enqueue(sessionId, () =>
        {
            var now = Interlocked.Increment(ref overlap);
            maxOverlap = Math.Max(maxOverlap, now);
            order.Add(2);
            Interlocked.Decrement(ref overlap);
            done.SetResult();
            return Task.CompletedTask;
        });

        done.Task.IsCompleted.ShouldBeFalse();
        releaseFirst.SetResult();
        await done.Task.WaitAsync(TimeSpan.FromSeconds(2));

        order.ShouldBe(new[] { 1, 2 });
        maxOverlap.ShouldBe(1);

        var idleDeadline = DateTime.UtcNow.AddSeconds(2);
        while (lanes.ActiveLaneCount != 0 && DateTime.UtcNow < idleDeadline)
        {
            await Task.Delay(10);
        }

        lanes.ActiveLaneCount.ShouldBe(0);
    }

    [Fact]
    public async Task Concurrent_enqueues_on_one_session_all_run_once_and_in_order()
    {
        var lanes = new ChatAssistantSessionLanes();
        var sessionId = Guid.NewGuid();
        const int count = 40;
        var order = new List<int>();
        var gate = new object();
        var overlap = 0;
        var maxOverlap = 0;
        var done = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        var enqueueTasks = Enumerable.Range(0, count).Select(index => Task.Run(() =>
        {
            lanes.Enqueue(sessionId, async () =>
            {
                var now = Interlocked.Increment(ref overlap);
                lock (gate)
                {
                    maxOverlap = Math.Max(maxOverlap, now);
                    order.Add(index);
                }

                await Task.Yield();
                Interlocked.Decrement(ref overlap);
                if (order.Count == count)
                {
                    done.TrySetResult();
                }
            });
        })).ToArray();

        await Task.WhenAll(enqueueTasks);
        await done.Task.WaitAsync(TimeSpan.FromSeconds(5));

        order.Count.ShouldBe(count);
        order.ToHashSet().Count.ShouldBe(count);
        maxOverlap.ShouldBe(1);
    }
}
