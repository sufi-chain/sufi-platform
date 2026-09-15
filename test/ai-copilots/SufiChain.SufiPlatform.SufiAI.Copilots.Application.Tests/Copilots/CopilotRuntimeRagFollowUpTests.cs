using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Copilots.Copilots;

public partial class CopilotRuntimeRagWorkspaceTests
{
    [Fact]
    public async Task Should_Keep_Raw_Hits_Without_Contextual_Retry()
    {
        var projectId = Guid.NewGuid();
        var fixture = CreateFixture(projectId, [projectId], IndexingWorkspaceName);
        var input = CreateFollowUpInput(fixture, "لایسنسش چیه؟");

        var result = await fixture.Orchestrator.PrepareRequestAsync(fixture.Definition, input);

        result.RagSearch.QueryMode.ShouldBe(CopilotRagQueryMode.Raw);
        result.RagSearch.Outcome.ShouldBe(CopilotRagSearchOutcome.Retrieved);
        result.RagSearch.AttemptCount.ShouldBe(1);
        await fixture.Rag.Received(1).SearchAsync(
            Arg.Is<SufiAIRagSearchRequest>(request => request.Query == input.Message), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("من یه شرکت تولید و پخش رادیاتور دارم و 50 نفر کارمند تو تیم های متفاوت دارم میشه بگی این پلتفرم چطور به کسب و کار من کمک میکنه")]
    [InlineData("لایسنسش چیه؟")]
    public async Task Should_Retry_Empty_FollowUp_Once_Within_Original_Project_Scope(string message)
    {
        var projectId = Guid.NewGuid();
        var fixture = CreateFixture(projectId, [projectId], IndexingWorkspaceName);
        var input = CreateFollowUpInput(fixture, message);
        var queries = new List<string>();
        fixture.Rag.SearchAsync(Arg.Any<SufiAIRagSearchRequest>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var request = call.Arg<SufiAIRagSearchRequest>();
                queries.Add(request.Query);
                request.WorkspaceName.ShouldBe(IndexingWorkspaceName);
                request.SourceName.ShouldBe("KnowledgeBase");
                request.MetadataFilters["projectId"].ShouldBe(projectId.ToString("D"));
                request.MinSimilarity.ShouldBe(CopilotRagRuntimeOptionsDefaults.SearchMinSimilarity);
                return request.Query == message ? new SufiAIRagSearchResult() : CreateRetrievedPassage();
            });

        var result = await fixture.Orchestrator.PrepareRequestAsync(fixture.Definition, input);

        queries.Count.ShouldBe(2);
        queries[0].ShouldBe(message);
        queries[1].ShouldContain("سکو صوفی چیه؟");
        queries[1].ShouldContain(message);
        queries[1].ShouldNotContain("assistant-only-invention");
        result.UsedRag.ShouldBeTrue();
        result.RetrievedChunkCount.ShouldBe(1);
        result.RagSearch.QueryMode.ShouldBe(CopilotRagQueryMode.ContextualRetry);
        result.RagSearch.Outcome.ShouldBe(CopilotRagSearchOutcome.Retrieved);
        result.RagSearch.AttemptCount.ShouldBe(2);
        result.Request.SystemPrompt.ShouldContain("Retrieved context:");
        result.Request.SystemPrompt.ShouldContain("Fresh indexed passage");
        result.Request.Messages.Last().Content.ShouldBe(message);
        fixture.Definition.SystemPrompt.ShouldBe("System instructions");
        await AssertSingleSearchProgressAsync(fixture, "1");
    }

    [Theory]
    [InlineData("سلام")]
    [InlineData("ممنون")]
    [InlineData("ok")]
    [InlineData("👍")]
    [InlineData("قیمت دلار چنده؟")]
    public async Task Should_Still_Search_Social_Or_New_Topic_Turns_Without_Retry(string message)
    {
        var projectId = Guid.NewGuid();
        var fixture = CreateFixture(projectId, [projectId], IndexingWorkspaceName);
        fixture.Rag.SearchAsync(Arg.Any<SufiAIRagSearchRequest>(), Arg.Any<CancellationToken>())
            .Returns(new SufiAIRagSearchResult());

        var result = await fixture.Orchestrator.PrepareRequestAsync(fixture.Definition, CreateFollowUpInput(fixture, message));

        result.RagSearch.AttemptCount.ShouldBe(1);
        result.RagSearch.QueryMode.ShouldBe(CopilotRagQueryMode.Raw);
        result.RagSearch.Outcome.ShouldBe(CopilotRagSearchOutcome.NoMatches);
        result.UsedRag.ShouldBeFalse();
        result.Request.SystemPrompt.ShouldNotContain("Retrieved context:");
        await fixture.Rag.Received(1).SearchAsync(Arg.Any<SufiAIRagSearchRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Keep_History_But_Not_Inject_Rag_When_Both_Searches_Miss()
    {
        var projectId = Guid.NewGuid();
        var fixture = CreateFixture(projectId, [projectId], IndexingWorkspaceName);
        fixture.Rag.SearchAsync(Arg.Any<SufiAIRagSearchRequest>(), Arg.Any<CancellationToken>())
            .Returns(new SufiAIRagSearchResult());

        var result = await fixture.Orchestrator.PrepareRequestAsync(fixture.Definition, CreateFollowUpInput(fixture, "لایسنسش چیه؟"));

        result.UsedRag.ShouldBeFalse();
        result.RetrievedChunkCount.ShouldBe(0);
        result.RagSearch.AttemptCount.ShouldBe(2);
        result.RagSearch.Outcome.ShouldBe(CopilotRagSearchOutcome.NoMatches);
        result.Request.SystemPrompt.ShouldNotContain("Retrieved context:");
        result.Request.Messages.ShouldContain(message => message.Role == "assistant" && message.Content == "assistant-only-invention");
        await fixture.Rag.Received(2).SearchAsync(Arg.Any<SufiAIRagSearchRequest>(), Arg.Any<CancellationToken>());
        await AssertSingleSearchProgressAsync(fixture, "0");
    }

    [Fact]
    public async Task Should_Expand_With_Successful_Query_And_Preserve_All_Metadata_Filters()
    {
        var projectId = Guid.NewGuid();
        var articleId = Guid.NewGuid().ToString("D");
        var fixture = CreateFixture(projectId, [projectId], IndexingWorkspaceName);
        fixture.Definition.SetRuntimeOptions(new CopilotRuntimeOptions
        {
            UseRag = true,
            RagSourceName = "KnowledgeBase",
            RagFilterByProjectId = true,
            RagMetadataFilters = new Dictionary<string, string> { ["culture"] = "fa", ["visibility"] = "public" }
        });
        var input = CreateFollowUpInput(fixture, "این پلتفرم چطور به شرکت من کمک میکنه؟");
        var expectedQuery = CopilotRagFollowUpQueryBuilder.TryBuild(input);
        fixture.Rag.SearchAsync(Arg.Any<SufiAIRagSearchRequest>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var request = call.Arg<SufiAIRagSearchRequest>();
                request.MetadataFilters["projectId"].ShouldBe(projectId.ToString("D"));
                request.MetadataFilters["culture"].ShouldBe("fa");
                request.MetadataFilters["visibility"].ShouldBe("public");
                request.WorkspaceName.ShouldBe(IndexingWorkspaceName);
                request.SourceName.ShouldBe("KnowledgeBase");
                if (request.Query == input.Message)
                {
                    return new SufiAIRagSearchResult();
                }

                request.Query.ShouldBe(expectedQuery);
                if (request.MetadataFilters.ContainsKey("articleId"))
                {
                    request.MetadataFilters["articleId"].ShouldBe(articleId);
                    request.MinSimilarity.ShouldBe(CopilotRagRuntimeOptionsDefaults.ArticleExpandMinSimilarity);
                }

                return CreateRetrievedPassage(articleId);
            });

        var result = await fixture.Orchestrator.PrepareRequestAsync(fixture.Definition, input);

        result.RagSearch.AttemptCount.ShouldBe(2);
        result.RetrievedChunkCount.ShouldBe(1);
        await fixture.Rag.Received(3).SearchAsync(Arg.Any<SufiAIRagSearchRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Not_Retry_When_Cancelled_After_Empty_Raw_Search()
    {
        var projectId = Guid.NewGuid();
        var fixture = CreateFixture(projectId, [projectId], IndexingWorkspaceName);
        using var cancellation = new CancellationTokenSource();
        fixture.Rag.SearchAsync(Arg.Any<SufiAIRagSearchRequest>(), Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                cancellation.Cancel();
                return new SufiAIRagSearchResult();
            });

        await Should.ThrowAsync<OperationCanceledException>(() => fixture.Orchestrator.PrepareRequestAsync(
            fixture.Definition, CreateFollowUpInput(fixture, "لایسنسش چیه؟"), cancellationToken: cancellation.Token));

        await fixture.Rag.Received(1).SearchAsync(Arg.Any<SufiAIRagSearchRequest>(), Arg.Any<CancellationToken>());
        await fixture.ProgressReporter.DidNotReceive().ReportAsync(
            Arg.Is<CopilotTurnProgressDto>(progress => progress.Stage == CopilotTurnProgressStages.SearchingKb && progress.Status == CopilotTurnProgressStatuses.Succeeded),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Should_Report_Failed_Search_And_Propagate_Raw_Or_Retry_Failure(bool failRetry)
    {
        var projectId = Guid.NewGuid();
        var fixture = CreateFixture(projectId, [projectId], IndexingWorkspaceName);
        var input = CreateFollowUpInput(fixture, "لایسنسش چیه؟");
        fixture.Rag.SearchAsync(Arg.Any<SufiAIRagSearchRequest>(), Arg.Any<CancellationToken>())
            .Returns(call => failRetry && call.Arg<SufiAIRagSearchRequest>().Query == input.Message
                ? Task.FromResult(new SufiAIRagSearchResult())
                : Task.FromException<SufiAIRagSearchResult>(new InvalidOperationException("Search provider unavailable")));

        await Should.ThrowAsync<InvalidOperationException>(() => fixture.Orchestrator.PrepareRequestAsync(fixture.Definition, input));

        await fixture.Rag.Received(failRetry ? 2 : 1).SearchAsync(Arg.Any<SufiAIRagSearchRequest>(), Arg.Any<CancellationToken>());
        await fixture.ProgressReporter.Received(1).ReportAsync(
            Arg.Is<CopilotTurnProgressDto>(progress => progress.Stage == CopilotTurnProgressStages.SearchingKb && progress.Status == CopilotTurnProgressStatuses.Started),
            Arg.Any<CancellationToken>());
        await fixture.ProgressReporter.Received(1).ReportAsync(
            Arg.Is<CopilotTurnProgressDto>(progress => progress.Stage == CopilotTurnProgressStages.SearchingKb && progress.Status == CopilotTurnProgressStatuses.Failed),
            Arg.Any<CancellationToken>());
        await fixture.ProgressReporter.DidNotReceive().ReportAsync(
            Arg.Is<CopilotTurnProgressDto>(progress => progress.Stage == CopilotTurnProgressStages.SearchingKb && progress.Status == CopilotTurnProgressStatuses.Succeeded),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Report_NotAttempted_When_Rag_Is_Disabled()
    {
        var projectId = Guid.NewGuid();
        var fixture = CreateFixture(projectId, [projectId], IndexingWorkspaceName);
        fixture.Definition.SetRuntimeOptions(new CopilotRuntimeOptions { UseRag = false });

        var result = await fixture.Orchestrator.PrepareRequestAsync(fixture.Definition, CreateFollowUpInput(fixture, "لایسنسش چیه؟"));

        result.RagSearch.Outcome.ShouldBe(CopilotRagSearchOutcome.NotAttempted);
        result.RagSearch.AttemptCount.ShouldBe(0);
        await fixture.Rag.DidNotReceive().SearchAsync(Arg.Any<SufiAIRagSearchRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Report_Provider_Timeout_As_Failed_When_Caller_Did_Not_Cancel()
    {
        var projectId = Guid.NewGuid();
        var fixture = CreateFixture(projectId, [projectId], IndexingWorkspaceName);
        fixture.Rag.SearchAsync(Arg.Any<SufiAIRagSearchRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<SufiAIRagSearchResult>(new OperationCanceledException("Provider timeout")));

        await Should.ThrowAsync<OperationCanceledException>(() => fixture.Orchestrator.PrepareRequestAsync(
            fixture.Definition, CreateFollowUpInput(fixture, "لایسنسش چیه؟")));

        await fixture.Rag.Received(1).SearchAsync(Arg.Any<SufiAIRagSearchRequest>(), Arg.Any<CancellationToken>());
        await fixture.ProgressReporter.Received(1).ReportAsync(
            Arg.Is<CopilotTurnProgressDto>(progress => progress.Stage == CopilotTurnProgressStages.SearchingKb && progress.Status == CopilotTurnProgressStatuses.Failed),
            Arg.Any<CancellationToken>());
    }

    private static CopilotRuntimeRequestDto CreateFollowUpInput(Fixture fixture, string message)
    {
        return new CopilotRuntimeRequestDto
        {
            CopilotId = fixture.Definition.Id,
            Message = message,
            ConversationHistory =
            [
                new() { Role = "user", Content = "سکو صوفی چیه؟" },
                new() { Role = "assistant", Content = "assistant-only-invention" }
            ]
        };
    }

    private static SufiAIRagSearchResult CreateRetrievedPassage(string? articleId = null)
    {
        return new SufiAIRagSearchResult
        {
            Chunks =
            [
                new()
                {
                    Id = "fresh-passage",
                    Content = "Fresh indexed passage",
                    SourceName = "KnowledgeBase",
                    Score = 0.6f,
                    Metadata = articleId == null
                        ? new Dictionary<string, object>()
                        : new Dictionary<string, object> { ["articleId"] = articleId }
                }
            ]
        };
    }

    private static async Task AssertSingleSearchProgressAsync(Fixture fixture, string detail)
    {
        await fixture.ProgressReporter.Received(1).ReportAsync(
            Arg.Is<CopilotTurnProgressDto>(progress => progress.Stage == CopilotTurnProgressStages.SearchingKb && progress.Status == CopilotTurnProgressStatuses.Started),
            Arg.Any<CancellationToken>());
        await fixture.ProgressReporter.Received(1).ReportAsync(
            Arg.Is<CopilotTurnProgressDto>(progress => progress.Stage == CopilotTurnProgressStages.SearchingKb && progress.Status == CopilotTurnProgressStatuses.Succeeded && progress.Detail == detail),
            Arg.Any<CancellationToken>());
        await fixture.ProgressReporter.DidNotReceive().ReportAsync(
            Arg.Is<CopilotTurnProgressDto>(progress => progress.Stage == CopilotTurnProgressStages.SearchingKb && progress.Status == CopilotTurnProgressStatuses.Failed),
            Arg.Any<CancellationToken>());
    }
}
