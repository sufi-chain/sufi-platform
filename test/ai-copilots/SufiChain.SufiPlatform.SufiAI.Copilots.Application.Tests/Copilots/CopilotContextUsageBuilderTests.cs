using System.Linq.Expressions;
using NSubstitute;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Copilots.Copilots;

public class CopilotContextUsageBuilderTests
{
    [Fact]
    public async Task Should_Use_Only_Latest_Turn_And_Group_Its_Conversation_Segments()
    {
        var session = CreateSession();
        var earlier = CreateTurn(session, new DateTime(2026, 9, 5, 10, 0, 0));
        var latest = CreateTurn(session, earlier.CreationTime.AddMinutes(1));
        var foreign = CreateTurn(CreateSession(), latest.CreationTime.AddMinutes(1));
        var segments = new List<CopilotContextSegment>
        {
            CreateSegment(session.Id, earlier.Id, CopilotContextSegmentKeys.Metadata, 90000, 70),
            CreateSegment(session.Id, latest.Id, CopilotContextSegmentKeys.Conversation, 400, 30),
            CreateSegment(session.Id, latest.Id, CopilotContextSegmentKeys.Conversation, 100, 30),
            CreateSegment(session.Id, latest.Id, CopilotContextSegmentKeys.SystemPrompt, 100, 10),
            CreateSegment(session.Id, latest.Id, CopilotContextSegmentKeys.Metadata, 20, 70),
            CreateSegment(session.Id, null, CopilotContextSegmentKeys.Metadata, 90000, 70),
            CreateSegment(foreign.SessionId, foreign.Id, CopilotContextSegmentKeys.Metadata, 90000, 70)
        };

        var usage = await BuildUsageAsync(session, [latest, foreign, earlier], segments);

        usage.UsedTokens.ShouldBe(620);
        usage.UsedPercent.ShouldBe(6.2m);
        usage.WarningState.ShouldBe("Normal");
        usage.SummarizationRequired.ShouldBeFalse();
        usage.Segments.Select(segment => segment.Key).ShouldBe(new[]
        {
            CopilotContextSegmentKeys.SystemPrompt,
            CopilotContextSegmentKeys.Conversation,
            CopilotContextSegmentKeys.Metadata
        });
        usage.Segments[1].EstimatedTokens.ShouldBe(500);
        usage.Segments[1].Percent.ShouldBe(5m);
        usage.Segments[1].SortOrder.ShouldBe(30);
        segments.Count.ShouldBe(7);
    }

    [Fact]
    public async Task Should_Not_Fall_Back_To_Older_Segments_When_Latest_Turn_Is_Empty()
    {
        var session = CreateSession();
        var earlier = CreateTurn(session, DateTime.UtcNow.AddMinutes(-1));
        var latest = CreateTurn(session, earlier.CreationTime.AddMinutes(1));

        var usage = await BuildUsageAsync(session, [earlier, latest],
            [CreateSegment(session.Id, earlier.Id, CopilotContextSegmentKeys.Metadata, 90000, 70)]);

        usage.UsedTokens.ShouldBe(0);
        usage.Segments.ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_Support_Empty_And_Carried_Summary_Sessions()
    {
        var session = CreateSession();
        (await BuildUsageAsync(session, [], [])).UsedTokens.ShouldBe(0);

        session.SetSummary("Carried summary text");
        var usage = await BuildUsageAsync(session, [], []);

        usage.SummaryVersion.ShouldBe(1);
        usage.Segments.Single().Key.ShouldBe(CopilotContextSegmentKeys.Summary);
        usage.UsedTokens.ShouldBe(5);
    }

    [Fact]
    public async Task Should_Not_Count_Summary_Twice()
    {
        var session = CreateSession();
        var turn = CreateTurn(session, DateTime.UtcNow);
        session.SetSummary("Carried summary text");

        var usage = await BuildUsageAsync(session, [turn],
            [CreateSegment(session.Id, turn.Id, CopilotContextSegmentKeys.Summary, 12, 20)]);

        usage.UsedTokens.ShouldBe(12);
        usage.Segments.Count.ShouldBe(1);
    }

    private static Task<CopilotContextUsageDto> BuildUsageAsync(
        CopilotContextSession session,
        List<CopilotContextTurn> turns,
        List<CopilotContextSegment> segments)
    {
        var turnRepository = Substitute.For<IRepository<CopilotContextTurn, Guid>>();
        turnRepository.GetListAsync(Arg.Any<Expression<Func<CopilotContextTurn, bool>>>(),
                Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(call => turns.Where(call.Arg<Expression<Func<CopilotContextTurn, bool>>>().Compile()).ToList());
        var segmentRepository = Substitute.For<IRepository<CopilotContextSegment, Guid>>();
        segmentRepository.GetListAsync(Arg.Any<Expression<Func<CopilotContextSegment, bool>>>(),
                Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(call => segments.Where(call.Arg<Expression<Func<CopilotContextSegment, bool>>>().Compile()).ToList());

        return CopilotContextUsageBuilder.BuildAsync(
            session, turnRepository, segmentRepository, new CopilotContextOptions());
    }

    private static CopilotContextSession CreateSession()
    {
        return new CopilotContextSession(Guid.NewGuid(), null, Guid.NewGuid(), Guid.NewGuid(),
            "workspace", "model", 10000);
    }

    private static CopilotContextTurn CreateTurn(CopilotContextSession session, DateTime creationTime)
    {
        return new CopilotContextTurn(Guid.NewGuid(), null, session.Id, session.CopilotId,
            session.WorkspaceId, session.WorkspaceName, session.Model, 0, false, false, 0, "Success")
        {
            CreationTime = creationTime
        };
    }

    private static CopilotContextSegment CreateSegment(Guid sessionId, Guid? turnId, string key, int tokens, int order)
    {
        return new CopilotContextSegment(Guid.NewGuid(), null, sessionId, turnId, key, key, tokens, order);
    }
}
