using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.SufiCom.Chat.Application.Messages;
using SufiChain.SufiPlatform.SufiCom.Chat.ETOs;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Chat.Messages;

public class ChatAssistantTurnQueueTests
{
    [Fact]
    public async Task Enqueue_runs_the_processor_in_a_new_scope_and_keeps_going_after_a_failure()
    {
        var calls = new List<Guid>();
        var secondDone = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var processor = Substitute.For<IChatAssistantTurnProcessor>();
        processor.ProcessAsync(Arg.Any<ChatMessageSentEto>()).Returns(call =>
        {
            var eto = call.Arg<ChatMessageSentEto>();
            calls.Add(eto.MessageId);
            if (calls.Count == 1)
            {
                throw new InvalidOperationException("boom");
            }

            secondDone.SetResult();
            return Task.CompletedTask;
        });

        var services = new ServiceCollection();
        services.AddScoped(_ => processor);
        using var provider = services.BuildServiceProvider();
        var queue = new ChatAssistantTurnQueue(
            provider.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<ChatAssistantTurnQueue>.Instance);

        var sessionId = Guid.NewGuid();
        var first = new ChatMessageSentEto { SessionId = sessionId, MessageId = Guid.NewGuid() };
        var second = new ChatMessageSentEto { SessionId = sessionId, MessageId = Guid.NewGuid() };

        queue.Enqueue(first);
        queue.Enqueue(second);

        await secondDone.Task.WaitAsync(TimeSpan.FromSeconds(2));
        calls.ShouldBe(new[] { first.MessageId, second.MessageId });
    }

    [Fact]
    public async Task A_second_message_enqueued_while_the_first_turn_is_running_is_answered_in_order()
    {
        var calls = new List<Guid>();
        var firstStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseFirst = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var secondDone = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var processor = Substitute.For<IChatAssistantTurnProcessor>();
        processor.ProcessAsync(Arg.Any<ChatMessageSentEto>()).Returns(call =>
        {
            var eto = call.Arg<ChatMessageSentEto>();
            calls.Add(eto.MessageId);
            if (calls.Count == 1)
            {
                firstStarted.SetResult();
                return releaseFirst.Task;
            }

            secondDone.SetResult();
            return Task.CompletedTask;
        });

        var services = new ServiceCollection();
        services.AddScoped(_ => processor);
        using var provider = services.BuildServiceProvider();
        var queue = new ChatAssistantTurnQueue(
            provider.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<ChatAssistantTurnQueue>.Instance);

        var sessionId = Guid.NewGuid();
        var firstId = Guid.NewGuid();
        var secondId = Guid.NewGuid();
        var first = new ChatMessageSentEto { SessionId = sessionId, MessageId = firstId, Id = Guid.NewGuid() };
        var second = new ChatMessageSentEto { SessionId = sessionId, MessageId = secondId, Id = Guid.NewGuid() };

        queue.Enqueue(first);
        await firstStarted.Task.WaitAsync(TimeSpan.FromSeconds(2));
        secondDone.Task.IsCompleted.ShouldBeFalse();

        queue.Enqueue(second);
        releaseFirst.SetResult();

        await secondDone.Task.WaitAsync(TimeSpan.FromSeconds(2));
        calls.ShouldBe(new[] { firstId, secondId });
    }

    [Fact]
    public async Task Enqueue_keeps_the_message_id_when_the_original_event_is_changed_before_the_turn_reads_it()
    {
        var readGate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var seen = new List<Guid>();
        var done = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var processor = Substitute.For<IChatAssistantTurnProcessor>();
        processor.ProcessAsync(Arg.Any<ChatMessageSentEto>()).Returns(async call =>
        {
            await readGate.Task;
            seen.Add(call.Arg<ChatMessageSentEto>().MessageId);
            done.SetResult();
        });

        var services = new ServiceCollection();
        services.AddScoped(_ => processor);
        using var provider = services.BuildServiceProvider();
        var queue = new ChatAssistantTurnQueue(
            provider.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<ChatAssistantTurnQueue>.Instance);

        var sessionId = Guid.NewGuid();
        var originalMessageId = Guid.NewGuid();
        var first = new ChatMessageSentEto
        {
            SessionId = sessionId,
            MessageId = originalMessageId,
            Id = Guid.NewGuid()
        };

        queue.Enqueue(first);
        first.MessageId = Guid.NewGuid();
        first.Id = Guid.NewGuid();
        readGate.SetResult();

        await done.Task.WaitAsync(TimeSpan.FromSeconds(2));
        seen.ShouldBe(new[] { originalMessageId });
    }
}

public class ChatAssistantTurnProcessorRegistrationTests : ChatTestBase<SufiComChatApplicationTestModule>
{
    [Fact]
    public void The_assistant_handler_is_registered_as_the_turn_processor()
    {
        GetRequiredService<IChatAssistantTurnProcessor>().ShouldBeOfType<ChatAssistantMessageHandler>();
    }
}
