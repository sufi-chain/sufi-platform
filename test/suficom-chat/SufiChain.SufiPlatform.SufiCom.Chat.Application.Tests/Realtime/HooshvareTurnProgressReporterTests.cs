using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;
using Volo.Abp.Timing;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Chat.Realtime;

public class HooshvareTurnProgressReporterTests
{
    [Fact]
    public async Task Transient_turn_routes_to_owner_and_restores_the_chat_scope()
    {
        var accessor = new HooshvareTurnProgressAccessor();
        var notifier = Substitute.For<IChatRealtimeNotifier>();
        var clock = Substitute.For<IClock>();
        var reporter = new ChatHooshvareTurnProgressReporter(accessor, notifier, clock);
        var sessionId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var turnId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var progress = new HooshvareTurnProgressDto
        {
            Stage = HooshvareTurnProgressStages.CallingTool,
            ToolName = "forms.get_designer_catalog"
        };

        await reporter.ReportAsync(progress);
        notifier.ReceivedCalls().ShouldBeEmpty();
        using (accessor.Begin(sessionId, tenantId))
        {
            using (accessor.BeginTurn(turnId, userId, tenantId))
                await reporter.ReportAsync(progress);

            accessor.Current!.ChatSessionId.ShouldBe(sessionId);
            await reporter.ReportAsync(progress);
        }
        accessor.Current.ShouldBeNull();
        await notifier.Received(1).NotifyHooshvareTurnProgressAsync(userId,
            Arg.Is<ChatAssistantProgressDto>(p => p.TurnId == turnId && p.TenantId == tenantId
                && p.SessionId == Guid.Empty && p.ToolName == progress.ToolName));
        await notifier.Received(1).NotifyAssistantProgressAsync(
            Arg.Is<ChatAssistantProgressDto>(p => p.SessionId == sessionId && p.TurnId == null));
    }

    [Fact]
    public async Task Concurrent_turns_keep_their_own_ambient_scope()
    {
        var accessor = new HooshvareTurnProgressAccessor();
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();
        using (accessor.BeginTurn(first, Guid.NewGuid()))
        {
            await Task.Run(async () =>
            {
                using (accessor.BeginTurn(second, Guid.NewGuid()))
                {
                    await Task.Yield();
                    accessor.Current!.TurnId.ShouldBe(second);
                }
            });
            accessor.Current!.TurnId.ShouldBe(first);
        }
        accessor.Current.ShouldBeNull();
    }
}
