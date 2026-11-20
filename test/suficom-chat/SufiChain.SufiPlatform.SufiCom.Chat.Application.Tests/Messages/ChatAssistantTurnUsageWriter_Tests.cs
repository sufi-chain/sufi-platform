using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.SufiCom.Chat.Application.Messages;
using SufiChain.SufiPlatform.SufiCom.Chat.Usage;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Chat.Messages;

public class ChatAssistantTurnUsageWriter_Tests
{
    [Fact]
    public async Task Successful_assistant_turn_reserves_and_records_auto_reply_usage()
    {
        var reservationId = Guid.NewGuid();
        var guard = Substitute.For<IChatUsageGuard>();
        guard.ReserveCompletedAiUsageAsync(Arg.Any<Guid>(), ChatAiOperationKind.AutoReply, Arg.Any<CancellationToken>())
            .Returns(reservationId);

        var writer = new ChatAssistantTurnUsageWriter(guard, NullLogger<ChatAssistantTurnUsageWriter>.Instance);
        var sessionId = Guid.NewGuid();

        await writer.TryRecordAutoReplyAsync(sessionId, 11, 7, 18, "openai", "support");

        await guard.Received(1).ReserveCompletedAiUsageAsync(sessionId, ChatAiOperationKind.AutoReply, Arg.Any<CancellationToken>());
        await guard.Received(1).RecordAiUsageAsync(
            reservationId,
            Arg.Is<ChatAiUsageRecord>(record =>
                record.PromptTokens == 11 &&
                record.CompletionTokens == 7 &&
                record.TotalTokens == 18 &&
                record.ProviderName == "openai" &&
                record.WorkspaceName == "support"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Reserve_exception_retries_and_still_records_the_turn()
    {
        var reservationId = Guid.NewGuid();
        var guard = Substitute.For<IChatUsageGuard>();
        var calls = 0;
        guard.ReserveCompletedAiUsageAsync(Arg.Any<Guid>(), ChatAiOperationKind.AutoReply, Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                calls++;
                if (calls == 1)
                {
                    throw new InvalidOperationException("transient reserve failure");
                }

                return reservationId;
            });

        var writer = new ChatAssistantTurnUsageWriter(guard, NullLogger<ChatAssistantTurnUsageWriter>.Instance);
        var sessionId = Guid.NewGuid();

        await writer.TryRecordAutoReplyAsync(sessionId, 3, 2, 5, "openai", "support");

        calls.ShouldBe(2);
        await guard.Received(1).RecordAiUsageAsync(
            reservationId,
            Arg.Is<ChatAiUsageRecord>(record => record.TotalTokens == 5),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Recording_failure_does_not_escape()
    {
        var reservationId = Guid.NewGuid();
        var guard = Substitute.For<IChatUsageGuard>();
        guard.ReserveCompletedAiUsageAsync(Arg.Any<Guid>(), Arg.Any<ChatAiOperationKind>(), Arg.Any<CancellationToken>())
            .Returns(reservationId);
        guard.RecordAiUsageAsync(Arg.Any<Guid>(), Arg.Any<ChatAiUsageRecord>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new InvalidOperationException("usage store unavailable"));

        var writer = new ChatAssistantTurnUsageWriter(guard, NullLogger<ChatAssistantTurnUsageWriter>.Instance);

        await writer.TryRecordAutoReplyAsync(Guid.NewGuid(), 1, 1, 2, null, null);
    }
}
