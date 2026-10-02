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
}

public class ChatAssistantTurnProcessorRegistrationTests : ChatTestBase<SufiComChatApplicationTestModule>
{
    [Fact]
    public void The_assistant_handler_is_registered_as_the_turn_processor()
    {
        GetRequiredService<IChatAssistantTurnProcessor>().ShouldBeOfType<ChatAssistantMessageHandler>();
    }
}
