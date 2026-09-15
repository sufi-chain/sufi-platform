using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Volo.Abp;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Copilots.Copilots;

/// <summary>
/// Authorization for KnowledgeBase RAG is the CopilotRagProjectBinding plus the projectId vector filter.
/// The workspace that is searched is the project's indexing workspace, which may differ from the
/// copilot's chat workspace; workspace-name equality is never treated as an ACL.
/// </summary>
public partial class CopilotRuntimeRagWorkspaceTests
{
    private const string ChatWorkspaceName = "copilot-chat";
    private const string IndexingWorkspaceName = "helpdesk-rag-indexing";

    [Fact]
    public async Task Should_Search_Project_RagIndexing_Workspace_When_Bound_Even_If_Chat_Workspace_Differs()
    {
        var projectId = Guid.NewGuid();
        var fixture = CreateFixture(projectId, boundProjectIds: [projectId], indexingWorkspaceName: IndexingWorkspaceName);
        var input = new CopilotRuntimeRequestDto
        {
            CopilotId = fixture.Definition.Id,
            Message = "How do I reset my password?",
            MetadataJson = CopilotRequestContextMetadata.Merge(
                null,
                new Dictionary<string, string> { ["projectId"] = projectId.ToString("D") })
        };

        var result = await fixture.Orchestrator.PrepareRequestAsync(fixture.Definition, input);

        result.UsedRag.ShouldBeTrue();
        result.RetrievedChunkCount.ShouldBe(1);
        result.ResolvedRagProjectId.ShouldBe(projectId);
        result.ResolvedRagWorkspaceName.ShouldBe(IndexingWorkspaceName);
        result.WorkspaceBinding.WorkspaceName.ShouldBe(ChatWorkspaceName);
        result.Request.SystemPrompt.ShouldContain("Retrieved context:");
        result.Request.Messages.ShouldNotContain(message =>
            message.Role == "system" && (message.Content?.Contains("Retrieved context:") ?? false));

        await fixture.Rag.Received(1).SearchAsync(
            Arg.Is<SufiAIRagSearchRequest>(request =>
                request.WorkspaceName == IndexingWorkspaceName &&
                request.SourceName == "KnowledgeBase" &&
                request.MinSimilarity == CopilotRagRuntimeOptionsDefaults.SearchMinSimilarity &&
                request.MetadataFilters["projectId"] == projectId.ToString("D")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Fall_Back_To_Chat_Workspace_When_No_Indexing_Workspace_Is_Resolved()
    {
        var projectId = Guid.NewGuid();
        var fixture = CreateFixture(projectId, boundProjectIds: [projectId], indexingWorkspaceName: null);
        var input = new CopilotRuntimeRequestDto
        {
            CopilotId = fixture.Definition.Id,
            Message = "Question",
            MetadataJson = CopilotRequestContextMetadata.Merge(
                null,
                new Dictionary<string, string> { ["projectId"] = projectId.ToString("D") })
        };

        var result = await fixture.Orchestrator.PrepareRequestAsync(fixture.Definition, input);

        result.ResolvedRagWorkspaceName.ShouldBe(ChatWorkspaceName);
        await fixture.Rag.Received(1).SearchAsync(
            Arg.Is<SufiAIRagSearchRequest>(request =>
                request.WorkspaceName == ChatWorkspaceName &&
                request.MinSimilarity == CopilotRagRuntimeOptionsDefaults.SearchMinSimilarity &&
                request.MetadataFilters["projectId"] == projectId.ToString("D")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Search_Indexing_Workspace_When_RagSourceName_Is_Empty_But_Bindings_Exist()
    {
        var projectId = Guid.NewGuid();
        var fixture = CreateFixture(projectId, boundProjectIds: [projectId], indexingWorkspaceName: IndexingWorkspaceName);
        fixture.Definition.SetRuntimeOptions(new CopilotRuntimeOptions { UseRag = true });
        var input = new CopilotRuntimeRequestDto
        {
            CopilotId = fixture.Definition.Id,
            Message = "How do I reset my password?",
            MetadataJson = CopilotRequestContextMetadata.Merge(
                null,
                new Dictionary<string, string> { ["projectId"] = projectId.ToString("D") })
        };
        var configuration = new CopilotEditableConfigurationDto
        {
            SystemPrompt = "System instructions",
            WorkspaceId = fixture.Definition.WorkspaceId,
            UseRag = true,
            RagProjectIds = [projectId]
        };

        var result = await fixture.Orchestrator.PrepareRequestAsync(fixture.Definition, input, configuration);

        result.UsedRag.ShouldBeTrue();
        result.ResolvedRagWorkspaceName.ShouldBe(IndexingWorkspaceName);
        await fixture.Rag.Received(1).SearchAsync(
            Arg.Is<SufiAIRagSearchRequest>(request =>
                request.WorkspaceName == IndexingWorkspaceName &&
                request.SourceName == "KnowledgeBase" &&
                request.MinSimilarity == CopilotRagRuntimeOptionsDefaults.SearchMinSimilarity &&
                request.MetadataFilters["projectId"] == projectId.ToString("D")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Expand_Sibling_Chunks_Of_The_Hit_Article()
    {
        var projectId = Guid.NewGuid();
        var articleId = Guid.NewGuid().ToString("D");
        var fixture = CreateFixture(projectId, boundProjectIds: [projectId], indexingWorkspaceName: IndexingWorkspaceName);
        fixture.Rag.SearchAsync(Arg.Any<SufiAIRagSearchRequest>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var request = call.Arg<SufiAIRagSearchRequest>();
                if (request.MetadataFilters.ContainsKey("articleId"))
                {
                    return new SufiAIRagSearchResult
                    {
                        Chunks =
                        [
                            new SufiAIDocumentChunk
                            {
                                Id = "frag",
                                Content = "tags: sufi-platform",
                                SourceId = articleId,
                                Score = 0.51f,
                                Metadata = new Dictionary<string, object> { ["articleId"] = articleId }
                            },
                            new SufiAIDocumentChunk
                            {
                                Id = "def",
                                Content = "سکوی صوفی یک سیستم‌عامل کسب‌وکار متن‌باز است.",
                                SourceId = articleId,
                                Score = 0.42f,
                                Metadata = new Dictionary<string, object> { ["articleId"] = articleId }
                            }
                        ]
                    };
                }

                return new SufiAIRagSearchResult
                {
                    Chunks =
                    [
                        new SufiAIDocumentChunk
                        {
                            Id = "frag",
                            Content = "tags: sufi-platform",
                            SourceId = articleId,
                            Score = 0.51f,
                            Metadata = new Dictionary<string, object> { ["articleId"] = articleId }
                        }
                    ]
                };
            });

        var input = new CopilotRuntimeRequestDto
        {
            CopilotId = fixture.Definition.Id,
            Message = "سکو صوفی چیه؟",
            MetadataJson = CopilotRequestContextMetadata.Merge(
                null,
                new Dictionary<string, string> { ["projectId"] = projectId.ToString("D") })
        };

        var result = await fixture.Orchestrator.PrepareRequestAsync(fixture.Definition, input);

        result.RetrievedChunkCount.ShouldBe(2);
        result.Request.SystemPrompt.ShouldContain("Retrieved context:");
        result.Request.SystemPrompt.ShouldContain("سیستم‌عامل کسب‌وکار");
        result.Request.Messages.ShouldNotContain(message =>
            message.Role == "system" && (message.Content?.Contains("Retrieved context:") ?? false));
        await fixture.Rag.Received().SearchAsync(
            Arg.Is<SufiAIRagSearchRequest>(request =>
                request.MinSimilarity == CopilotRagRuntimeOptionsDefaults.ArticleExpandMinSimilarity &&
                request.MetadataFilters["articleId"] == articleId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Throw_UnboundRagProject_When_Requested_Project_Is_Not_Bound()
    {
        var boundProjectId = Guid.NewGuid();
        var requestedProjectId = Guid.NewGuid();
        var fixture = CreateFixture(requestedProjectId, boundProjectIds: [boundProjectId], indexingWorkspaceName: IndexingWorkspaceName);
        var input = new CopilotRuntimeRequestDto
        {
            CopilotId = fixture.Definition.Id,
            Message = "Question",
            MetadataJson = CopilotRequestContextMetadata.Merge(
                null,
                new Dictionary<string, string> { ["projectId"] = requestedProjectId.ToString("D") })
        };

        var exception = await Should.ThrowAsync<BusinessException>(
            () => fixture.Orchestrator.PrepareRequestAsync(fixture.Definition, input));

        exception.Code.ShouldBe("SufiAICopilots:UnboundRagProject");
        await fixture.Rag.DidNotReceive().SearchAsync(Arg.Any<SufiAIRagSearchRequest>(), Arg.Any<CancellationToken>());
        await fixture.IndexingWorkspaceResolver.DidNotReceive().ResolveAsync(
            Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Throw_RagProjectBindingRequired_When_Copilot_Has_No_Bindings()
    {
        var fixture = CreateFixture(Guid.NewGuid(), boundProjectIds: [], indexingWorkspaceName: IndexingWorkspaceName);
        var input = new CopilotRuntimeRequestDto
        {
            CopilotId = fixture.Definition.Id,
            Message = "Question"
        };

        var exception = await Should.ThrowAsync<BusinessException>(
            () => fixture.Orchestrator.PrepareRequestAsync(fixture.Definition, input));

        exception.Code.ShouldBe("SufiAICopilots:RagProjectBindingRequired");
        await fixture.Rag.DidNotReceive().SearchAsync(Arg.Any<SufiAIRagSearchRequest>(), Arg.Any<CancellationToken>());
    }

    private static Fixture CreateFixture(Guid projectId, Guid[] boundProjectIds, string? indexingWorkspaceName)
    {
        var definition = new CopilotDefinition(Guid.NewGuid(), null, "Tests", "HelpDesk assistant",
            CopilotKind.Assistant, "Assistant", Guid.NewGuid(), "System instructions", false);
        definition.SetRuntimeOptions(new CopilotRuntimeOptions
        {
            UseRag = true,
            RagSourceName = "KnowledgeBase",
            RagFilterByProjectId = true
        });

        var bindings = Substitute.For<ICopilotRagProjectBindingRepository>();
        bindings.GetListByCopilotAsync(definition.Id, "KnowledgeBase", Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(boundProjectIds
                .Select(id => new CopilotRagProjectBinding(Guid.NewGuid(), null, definition.Id, id, "KnowledgeBase"))
                .ToList());

        var rag = Substitute.For<ISufiAIRagService>();
        rag.SearchAsync(Arg.Any<SufiAIRagSearchRequest>(), Arg.Any<CancellationToken>())
            .Returns(new SufiAIRagSearchResult
            {
                Chunks =
                [
                    new SufiAIDocumentChunk
                    {
                        Id = "password-reset-passage",
                        Content = "Reset your password from the profile page.",
                        SourceName = "KnowledgeBase",
                        Score = 0.9f
                    }
                ]
            });

        var indexingWorkspaceResolver = Substitute.For<ICopilotRagIndexingWorkspaceResolver>();
        indexingWorkspaceResolver.ResolveAsync(projectId, "KnowledgeBase", Arg.Any<CancellationToken>())
            .Returns(indexingWorkspaceName == null
                ? null
                : new CopilotRagIndexingWorkspace
                {
                    ProjectId = projectId,
                    SourceName = "KnowledgeBase",
                    WorkspaceId = Guid.NewGuid(),
                    WorkspaceName = indexingWorkspaceName
                });

        var workspaceResolver = Substitute.For<ICopilotWorkspaceResolver>();
        workspaceResolver.ResolveAsync(definition.WorkspaceId, Arg.Any<CancellationToken>())
            .Returns(new CopilotWorkspaceBinding
            {
                WorkspaceId = definition.WorkspaceId,
                WorkspaceName = ChatWorkspaceName,
                IsActive = true,
                IsReady = true
            });

        var runtimeResolver = Substitute.For<IWorkspaceRuntimeConfigurationResolver>();
        runtimeResolver
            .ResolveAsync(Arg.Any<Guid>(), Arg.Any<AICapabilityType>(), Arg.Any<AIModelRouteSelection>(), Arg.Any<CancellationToken>())
            .Returns(new WorkspaceRuntimeConfiguration
            {
                Workspace = new Workspace(definition.WorkspaceId, ChatWorkspaceName, AIProviderType.OpenAI, "gpt-4"),
                CapabilityType = AICapabilityType.ChatCompletion,
                Provider = AIProviderType.OpenAI,
                ModelId = "gpt-4",
                OpenAIApiMode = OpenAIApiMode.ChatCompletions,
                IsConfigured = true,
                IsReady = true
            });

        var localizers = Substitute.For<IStringLocalizerFactory>();
        var progressReporter = Substitute.For<ICopilotTurnProgressReporter>();
        var orchestrator = new CopilotRuntimeOrchestrator(
            rag,
            bindings,
            Substitute.For<IMCPToolRegistry>(),
            new CopilotBusinessLocalizationService(localizers),
            localizers,
            workspaceResolver,
            runtimeResolver,
            new CopilotContextTokenEstimator(),
            progressReporter,
            Substitute.For<IWorkspaceGuardrailService>(),
            indexingWorkspaceResolver,
            Substitute.For<ICopilotContextFieldRegistry>(),
            NullLogger<CopilotRuntimeOrchestrator>.Instance);

        return new Fixture(definition, rag, indexingWorkspaceResolver, orchestrator, progressReporter);
    }

    private sealed record Fixture(
        CopilotDefinition Definition,
        ISufiAIRagService Rag,
        ICopilotRagIndexingWorkspaceResolver IndexingWorkspaceResolver,
        CopilotRuntimeOrchestrator Orchestrator,
        ICopilotTurnProgressReporter ProgressReporter);
}
