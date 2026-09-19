using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Copilots.Copilots;

public class CopilotRuntimeContextSegmentsTests
{
    [Fact]
    public async Task Should_Measure_Metadata_Text_And_Keep_Rag_Out_Of_Tool_Estimates()
    {
        var definition = new CopilotDefinition(Guid.NewGuid(), null, "Tests", "Test assistant",
            CopilotKind.Assistant, "Assistant", Guid.NewGuid(), "System instructions", false);
        var runtimeOptions = new CopilotRuntimeOptions
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
        var resolver = Substitute.For<ICopilotWorkspaceResolver>();
        resolver.ResolveAsync(definition.WorkspaceId, Arg.Any<CancellationToken>())
            .Returns(new CopilotWorkspaceBinding
            {
                WorkspaceId = definition.WorkspaceId,
                WorkspaceName = "workspace",
                IsActive = true,
                IsReady = true
            });
        var localizers = Substitute.For<IStringLocalizerFactory>();
        var estimator = new CopilotContextTokenEstimator();
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
        var orchestrator = new CopilotRuntimeOrchestrator(
            CopilotRuntimeTestSupport.CreateRagRetrieval(rag, searchKb: true), Substitute.For<ICopilotRagProjectBindingRepository>(), tools,
            new CopilotBusinessLocalizationService(localizers), localizers, resolver, runtimeResolver, estimator,
            Substitute.For<IWorkspaceGuardrailService>(),
            new NullCopilotRagIndexingWorkspaceResolver(),
            Substitute.For<ICopilotContextFieldRegistry>(),
            NullLogger<CopilotRuntimeOrchestrator>.Instance);
        const string title = "آزمایش فارسی";
        var input = new CopilotRuntimeRequestDto
        {
            CopilotId = definition.Id,
            Message = "Current message",
            ConversationHistory = [new CopilotChatMessageDto { Role = "user", Content = "Previous message" }],
            MetadataJson = CopilotRequestContextMetadata.Merge(
                "{\"unrelated\":\"not prompt context\"}", new Dictionary<string, string> { ["title"] = title })
        };

        var withoutRag = await orchestrator.PrepareRequestAsync(definition, input);
        runtimeOptions.UseRag = true;
        var withRag = await orchestrator.PrepareRequestAsync(definition, input);

        var toolsWithoutRag = withoutRag.Segments.Single(segment => segment.Key == CopilotContextSegmentKeys.McpTools);
        var toolsWithRag = withRag.Segments.Single(segment => segment.Key == CopilotContextSegmentKeys.McpTools);
        toolsWithRag.EstimatedTokens.ShouldBe(toolsWithoutRag.EstimatedTokens);
        toolsWithRag.SortOrder.ShouldBe(50);
        toolsWithRag.Percent.ShouldBe(0);
        withRag.Segments.ShouldContain(segment => segment.Key == CopilotContextSegmentKeys.RagKnowledge);

        var expectedContext = new System.Text.StringBuilder()
            .AppendLine()
            .AppendLine("copilotContext (authoritative for this user message only — do not reuse from conversation history):")
            .Append("- title: ").AppendLine(title).ToString();
        withRag.Segments.Single(segment => segment.Key == CopilotContextSegmentKeys.Metadata)
            .EstimatedTokens.ShouldBe(estimator.EstimateText(expectedContext));
        withRag.Request.SystemPrompt.ShouldContain(title);
        withRag.Request.SystemPrompt.ShouldNotContain("\\u0622");
        withRag.Request.SystemPrompt.ShouldNotContain("not prompt context");
        withRag.Request.SystemPrompt.ShouldNotContain("Previous message");
        withRag.Request.Messages.ShouldNotContain(message => message.Role == "system");
        withRag.Request.Messages.Count(message => message.Content == "Previous message").ShouldBe(1);
        withRag.Request.Messages.Count(message => message.Content == "Current message").ShouldBe(1);
    }
}
