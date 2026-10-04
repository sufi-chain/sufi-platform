using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI;
using Volo.Abp;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;

public class HooshvareRagRetrievalFallbackTests
{
    [Fact]
    public async Task Should_Search_TopK_With_The_User_Message_When_Planner_Json_Is_Invalid()
    {
        var projectId = Guid.NewGuid();
        var chat = Substitute.For<ISufiAIChatService>();
        chat.IsAvailableAsync(Arg.Any<CancellationToken>()).Returns(true);
        chat.CompleteAsync(Arg.Any<SufiAIChatRequest>(), Arg.Any<CancellationToken>())
            .Returns(new SufiAIChatResponse { Content = "I cannot produce JSON" });

        var rag = Substitute.For<ISufiAIRagService>();
        rag.SearchAsync(Arg.Any<SufiAIRagSearchRequest>(), Arg.Any<CancellationToken>())
            .Returns(new SufiAIRagSearchResult
            {
                Chunks =
                [
                    new SufiAIDocumentChunk
                    {
                        Id = "chunk-1",
                        Content = "Reset your password from the profile page.",
                        Score = 0.62f
                    }
                ]
            });

        var service = CreateService(rag, new HooshvareRagSearchPlanner(chat, NullLogger<HooshvareRagSearchPlanner>.Instance));
        var message = "how do I reset a password?";

        var result = await service.RetrieveAsync(CreateRequest(projectId, message));

        result.Diagnostics.QueryMode.ShouldBe(HooshvareRagQueryMode.Fallback);
        result.Diagnostics.Outcome.ShouldBe(HooshvareRagSearchOutcome.Retrieved);
        result.Diagnostics.ReasonCode.ShouldBe(HooshvareRagReasonCodes.PlannerPlanInvalid);
        result.Diagnostics.AttemptCount.ShouldBe(1);
        result.Chunks.Count.ShouldBe(1);
        await rag.Received(1).SearchAsync(
            Arg.Is<SufiAIRagSearchRequest>(request =>
                request.Query == message &&
                request.MaxResults == HooshvareRagRuntimeOptionsDefaults.DefaultTopK &&
                request.WorkspaceName == "rag-workspace" &&
                request.MetadataFilters["projectId"] == projectId.ToString("D")),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(HooshvareRagReasonCodes.EmbeddingsCredentialsMissing)]
    [InlineData(HooshvareRagReasonCodes.EmbeddingsModelNotConfigured)]
    public async Task Should_Report_Missing_Embeddings_Configuration_Without_Opening_The_Circuit(string reasonCode)
    {
        var projectId = Guid.NewGuid();
        var planner = Substitute.For<IHooshvareRagSearchPlanner>();
        planner.DecideAsync(Arg.Any<HooshvareRagPlannerRequest>(), Arg.Any<CancellationToken>())
            .Returns(new HooshvareRagPlannerDecision
            {
                SearchKb = true,
                Succeeded = true
            });

        var rag = Substitute.For<ISufiAIRagService>();
        rag.SearchAsync(Arg.Any<SufiAIRagSearchRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<SufiAIRagSearchResult>(new BusinessException(reasonCode)
                .WithData("WorkspaceName", "rag-workspace")));

        var circuit = Substitute.For<IHooshvareRagCircuitStore>();
        circuit.IsOpenAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        var service = CreateService(rag, planner, circuit);

        var result = await service.RetrieveAsync(CreateRequest(projectId, "how do I reset a password?"));

        result.Chunks.ShouldBeEmpty();
        result.Diagnostics.Outcome.ShouldBe(HooshvareRagSearchOutcome.Unavailable);
        result.Diagnostics.ReasonCode.ShouldBe(reasonCode);
        result.UnavailableNotice.ShouldNotBeNull();
        result.UnavailableNotice.ShouldNotContain(reasonCode);
        await circuit.DidNotReceive().OpenAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    private static HooshvareRagRetrievalService CreateService(
        ISufiAIRagService rag,
        IHooshvareRagSearchPlanner planner,
        IHooshvareRagCircuitStore? circuit = null)
    {
        circuit ??= Substitute.For<IHooshvareRagCircuitStore>();
        circuit.IsOpenAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        return new HooshvareRagRetrievalService(
            rag,
            planner,
            circuit,
            Substitute.For<IHooshvareTurnProgressReporter>(),
            NullLogger<HooshvareRagRetrievalService>.Instance);
    }

    private static HooshvareRagRetrievalRequest CreateRequest(Guid projectId, string message)
    {
        return new HooshvareRagRetrievalRequest
        {
            HooshvareId = Guid.NewGuid(),
            Input = new HooshvareRuntimeRequestDto { Message = message },
            Options = new HooshvareRuntimeOptions
            {
                UseRag = true,
                RagTopK = HooshvareRagRuntimeOptionsDefaults.DefaultTopK,
                RagSourceName = "HelpDesk.KnowledgeBase"
            },
            ChatWorkspaceName = "chat",
            RagWorkspaceName = "rag-workspace",
            ResolvedProjectId = projectId
        };
    }
}
