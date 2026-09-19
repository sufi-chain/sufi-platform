using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;

public class HooshvareRuntimeContextSegmentsTests
{
    [Fact]
    public async Task Should_Measure_Metadata_Text_And_Keep_Rag_Out_Of_Tool_Estimates()
    {
        var definition = new HooshvareDefinition(Guid.NewGuid(), null, "Tests", "Test assistant",
            HooshvareKind.Assistant, "Assistant", Guid.NewGuid(), "System instructions", false);
        var runtimeOptions = new HooshvareRuntimeOptions
        {
            UseMcpTools = true,
            AllowedMcpToolNames = ["test_tool"]
        };
        definition.SetRuntimeOptions(runtimeOptions);
        var tool = Substitute.For<IMCPTool>();
        tool.Name.Returns("test_tool");
        tool.Description.Returns("Test tool description");
        var tools = Substitute.For<IMCPToolRegistry>();
        tools.ResolveAsync(Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<CancellationToken>())
            .Returns(new MCPToolResolutionResult { Tools = [tool] });
        var rag = Substitute.For<ISufiAIRagService>();
        rag.SearchAsync(Arg.Any<SufiAIRagSearchRequest>(), Arg.Any<CancellationToken>())
            .Returns(new SufiAIRagSearchResult
            {
                Chunks = [new SufiAIDocumentChunk { Content = new string('R', 4000) }]
            });
        var resolver = Substitute.For<IHooshvareWorkspaceResolver>();
        resolver.ResolveAsync(definition.WorkspaceId, Arg.Any<CancellationToken>())
            .Returns(new HooshvareWorkspaceBinding
            {
                WorkspaceId = definition.WorkspaceId,
                WorkspaceName = "workspace",
                IsActive = true,
                IsReady = true
            });
        var localizers = Substitute.For<IStringLocalizerFactory>();
        var estimator = new HooshvareContextTokenEstimator();
        var runtimeResolver = Substitute.For<IWorkspaceRuntimeConfigurationResolver>();
        runtimeResolver
            .ResolveAsync(
                Arg.Any<Guid>(),
                Arg.Any<AICapabilityType>(),
                Arg.Any<AIModelRouteSelection>(),
                Arg.Any<CancellationToken>())
            .Returns(new WorkspaceRuntimeConfiguration
            {
                Workspace = new Workspace(definition.WorkspaceId, "workspace", AIProviderType.OpenAI, "gpt-4"),
                CapabilityType = AICapabilityType.ChatCompletion,
                Provider = AIProviderType.OpenAI,
                ModelId = "gpt-4",
                OpenAIApiMode = OpenAIApiMode.ChatCompletions,
                IsConfigured = true,
                IsReady = true
            });
        var orchestrator = new HooshvareRuntimeOrchestrator(
            HooshvareRuntimeTestSupport.CreateRagRetrieval(rag, searchKb: true), Substitute.For<IHooshvareRagProjectBindingRepository>(), tools,
            new HooshvareBusinessLocalizationService(localizers), localizers, resolver, runtimeResolver, estimator,
            Substitute.For<IWorkspaceGuardrailService>(),
            new NullHooshvareRagIndexingWorkspaceResolver(),
            Substitute.For<IHooshvareContextFieldRegistry>(),
            NullLogger<HooshvareRuntimeOrchestrator>.Instance);
        const string title = "آزمایش فارسی";
        var input = new HooshvareRuntimeRequestDto
        {
            HooshvareId = definition.Id,
            Message = "Current message",
            ConversationHistory = [new HooshvareChatMessageDto { Role = "user", Content = "Previous message" }],
            MetadataJson = HooshvareRequestContextMetadata.Merge(
                "{\"unrelated\":\"not prompt context\"}", new Dictionary<string, string> { ["title"] = title })
        };

        var withoutRag = await orchestrator.PrepareRequestAsync(definition, input);
        runtimeOptions.UseRag = true;
        var withRag = await orchestrator.PrepareRequestAsync(definition, input);

        var toolsWithoutRag = withoutRag.Segments.Single(segment => segment.Key == HooshvareContextSegmentKeys.McpTools);
        var toolsWithRag = withRag.Segments.Single(segment => segment.Key == HooshvareContextSegmentKeys.McpTools);
        toolsWithRag.EstimatedTokens.ShouldBe(toolsWithoutRag.EstimatedTokens);
        toolsWithRag.SortOrder.ShouldBe(50);
        toolsWithRag.Percent.ShouldBe(0);
        withRag.Segments.ShouldContain(segment => segment.Key == HooshvareContextSegmentKeys.RagKnowledge);

        var expectedContext = new System.Text.StringBuilder()
            .AppendLine()
            .AppendLine("hooshvareContext (authoritative for this user message only — do not reuse from conversation history):")
            .Append("- title: ").AppendLine(title).ToString();
        withRag.Segments.Single(segment => segment.Key == HooshvareContextSegmentKeys.Metadata)
            .EstimatedTokens.ShouldBe(estimator.EstimateText(expectedContext));
        withRag.Request.SystemPrompt!.ShouldContain(title);
        withRag.Request.SystemPrompt!.ShouldNotContain("\\u0622");
        withRag.Request.SystemPrompt!.ShouldNotContain("not prompt context");
        withRag.Request.SystemPrompt!.ShouldNotContain("Previous message");
        withRag.Request.Messages.ShouldNotContain(message => message.Role == "system");
        withRag.Request.Messages.Count(message => message.Content == "Previous message").ShouldBe(1);
        withRag.Request.Messages.Count(message => message.Content == "Current message").ShouldBe(1);
    }
}
