using Microsoft.Extensions.Localization;

using Microsoft.Extensions.Logging.Abstractions;

using NSubstitute;

using Shouldly;

using SufiChain.SufiPlatform.SufiAI;

using SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;

using SufiChain.SufiPlatform.SufiAI.Workspaces;

using Volo.Abp;

using Xunit;



namespace SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;



/// <summary>

/// Authorization for KnowledgeBase RAG is the HooshvareRagProjectBinding plus the projectId vector filter.

/// The workspace that is searched is the project's indexing workspace, which may differ from the

/// hooshvare's chat workspace; workspace-name equality is never treated as an ACL.

/// </summary>

public partial class HooshvareRuntimeRagWorkspaceTests

{

    private const string ChatWorkspaceName = "hooshvare-chat";

    private const string IndexingWorkspaceName = "helpdesk-rag-indexing";



    [Fact]

    public async Task Structured_response_contract_survives_orchestration_without_enabling_tools()

    {

        var fixture = CreateFixture(Guid.NewGuid(), [], null);

        fixture.Definition.SetRuntimeOptions(new HooshvareRuntimeOptions { UseRag = false, UseMcpTools = false });

        var schema = new SufiAIJsonResponseSchema

        {

            Name = "reply", SchemaJson = """{"type":"object","properties":{},"additionalProperties":false}"""

        };

        var result = await fixture.Orchestrator.PrepareRequestAsync(fixture.Definition, new HooshvareRuntimeRequestDto

        {

            HooshvareId = fixture.Definition.Id, Message = "Return JSON", ResponseSchema = schema

        });

        result.Request.ResponseSchema.ShouldBeSameAs(schema);

        result.UsedMcp.ShouldBeFalse();

        // The application request audit includes the effective schema.

        System.Text.Json.JsonSerializer.Serialize(result.Request).ShouldContain("ResponseSchema");

    }



    [Fact]

    public void Incoming_chat_json_cannot_override_the_server_response_contract()

    {

        const string json = """{"Message":"hi","ResponseSchema":{"Name":"untrusted","SchemaJson":"{}"}}""";

        System.Text.Json.JsonSerializer.Deserialize<HooshvareRuntimeRequestDto>(json)!.ResponseSchema.ShouldBeNull();

    }



    [Fact]

    public async Task Should_Search_Project_RagIndexing_Workspace_When_Bound_Even_If_Chat_Workspace_Differs()

    {

        var projectId = Guid.NewGuid();

        var fixture = CreateFixture(projectId, boundProjectIds: [projectId], indexingWorkspaceName: IndexingWorkspaceName);

        var input = new HooshvareRuntimeRequestDto

        {

            HooshvareId = fixture.Definition.Id,

            Message = "How do I reset my password?",

            MetadataJson = HooshvareRequestContextMetadata.Merge(

                null,

                new Dictionary<string, string> { ["projectId"] = projectId.ToString("D") })

        };



        var result = await fixture.Orchestrator.PrepareRequestAsync(fixture.Definition, input);



        result.UsedRag.ShouldBeTrue();

        result.RetrievedChunkCount.ShouldBe(1);

        result.ResolvedRagProjectId.ShouldBe(projectId);

        result.ResolvedRagWorkspaceName.ShouldBe(IndexingWorkspaceName);

        result.WorkspaceBinding.WorkspaceName.ShouldBe(ChatWorkspaceName);

        result.Request.SystemPrompt!.ShouldContain("Retrieved context:");

        result.Request.Messages.ShouldNotContain(message =>

            message.Role == "system" && message.Content != null && message.Content.Contains("Retrieved context:"));



        await fixture.Rag.Received(1).SearchAsync(

            Arg.Is<SufiAIRagSearchRequest>(request =>

                request.WorkspaceName == IndexingWorkspaceName &&

                request.SourceName == "KnowledgeBase" &&

                request.MinSimilarity == HooshvareRagRuntimeOptionsDefaults.SearchMinSimilarity &&

                request.MetadataFilters["projectId"] == projectId.ToString("D")),

            Arg.Any<CancellationToken>());

    }



    [Fact]

    public async Task Should_Fall_Back_To_Chat_Workspace_When_No_Indexing_Workspace_Is_Resolved()

    {

        var projectId = Guid.NewGuid();

        var fixture = CreateFixture(projectId, boundProjectIds: [projectId], indexingWorkspaceName: null);

        var input = new HooshvareRuntimeRequestDto

        {

            HooshvareId = fixture.Definition.Id,

            Message = "Question",

            MetadataJson = HooshvareRequestContextMetadata.Merge(

                null,

                new Dictionary<string, string> { ["projectId"] = projectId.ToString("D") })

        };



        var result = await fixture.Orchestrator.PrepareRequestAsync(fixture.Definition, input);



        result.ResolvedRagWorkspaceName.ShouldBe(ChatWorkspaceName);

        await fixture.Rag.Received(1).SearchAsync(

            Arg.Is<SufiAIRagSearchRequest>(request =>

                request.WorkspaceName == ChatWorkspaceName &&

                request.MinSimilarity == HooshvareRagRuntimeOptionsDefaults.SearchMinSimilarity &&

                request.MetadataFilters["projectId"] == projectId.ToString("D")),

            Arg.Any<CancellationToken>());

    }



    [Fact]

    public async Task Should_Search_Indexing_Workspace_When_RagSourceName_Is_Empty_But_Bindings_Exist()

    {

        var projectId = Guid.NewGuid();

        var fixture = CreateFixture(projectId, boundProjectIds: [projectId], indexingWorkspaceName: IndexingWorkspaceName);

        fixture.Definition.SetRuntimeOptions(new HooshvareRuntimeOptions { UseRag = true });

        var input = new HooshvareRuntimeRequestDto

        {

            HooshvareId = fixture.Definition.Id,

            Message = "How do I reset my password?",

            MetadataJson = HooshvareRequestContextMetadata.Merge(

                null,

                new Dictionary<string, string> { ["projectId"] = projectId.ToString("D") })

        };

        var configuration = new HooshvareEditableConfigurationDto

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

                request.MinSimilarity == HooshvareRagRuntimeOptionsDefaults.SearchMinSimilarity &&

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



        var input = new HooshvareRuntimeRequestDto

        {

            HooshvareId = fixture.Definition.Id,

            Message = "سکو صوفی چیه؟",

            MetadataJson = HooshvareRequestContextMetadata.Merge(

                null,

                new Dictionary<string, string> { ["projectId"] = projectId.ToString("D") })

        };



        var result = await fixture.Orchestrator.PrepareRequestAsync(fixture.Definition, input);



        result.RetrievedChunkCount.ShouldBe(2);

        result.Request.SystemPrompt!.ShouldContain("Retrieved context:");

        result.Request.SystemPrompt!.ShouldContain("سیستم‌عامل کسب‌وکار");

        result.Request.Messages.ShouldNotContain(message =>

            message.Role == "system" && message.Content != null && message.Content.Contains("Retrieved context:"));

        await fixture.Rag.Received().SearchAsync(

            Arg.Is<SufiAIRagSearchRequest>(request =>

                request.MinSimilarity == HooshvareRagRuntimeOptionsDefaults.ArticleExpandMinSimilarity &&

                request.MetadataFilters["articleId"] == articleId),

            Arg.Any<CancellationToken>());

    }



    [Fact]

    public async Task Should_Throw_UnboundRagProject_When_Requested_Project_Is_Not_Bound()

    {

        var boundProjectId = Guid.NewGuid();

        var requestedProjectId = Guid.NewGuid();

        var fixture = CreateFixture(requestedProjectId, boundProjectIds: [boundProjectId], indexingWorkspaceName: IndexingWorkspaceName);

        var input = new HooshvareRuntimeRequestDto

        {

            HooshvareId = fixture.Definition.Id,

            Message = "Question",

            MetadataJson = HooshvareRequestContextMetadata.Merge(

                null,

                new Dictionary<string, string> { ["projectId"] = requestedProjectId.ToString("D") })

        };



        var exception = await Should.ThrowAsync<BusinessException>(

            () => fixture.Orchestrator.PrepareRequestAsync(fixture.Definition, input));



        exception.Code.ShouldBe("SufiAIHooshvare:UnboundRagProject");

        await fixture.Rag.DidNotReceive().SearchAsync(Arg.Any<SufiAIRagSearchRequest>(), Arg.Any<CancellationToken>());

        await fixture.IndexingWorkspaceResolver.DidNotReceive().ResolveAsync(

            Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>());

    }



    [Fact]

    public async Task Should_Throw_RagProjectBindingRequired_When_Hooshvare_Has_No_Bindings()

    {

        var fixture = CreateFixture(Guid.NewGuid(), boundProjectIds: [], indexingWorkspaceName: IndexingWorkspaceName);

        var input = new HooshvareRuntimeRequestDto

        {

            HooshvareId = fixture.Definition.Id,

            Message = "Question"

        };



        var exception = await Should.ThrowAsync<BusinessException>(

            () => fixture.Orchestrator.PrepareRequestAsync(fixture.Definition, input));



        exception.Code.ShouldBe("SufiAIHooshvare:RagProjectBindingRequired");

        await fixture.Rag.DidNotReceive().SearchAsync(Arg.Any<SufiAIRagSearchRequest>(), Arg.Any<CancellationToken>());

    }



    [Fact]

    public async Task Should_Continue_Without_Throwing_When_Rag_Search_Fails()

    {

        var projectId = Guid.NewGuid();

        var fixture = CreateFixture(projectId, boundProjectIds: [projectId], indexingWorkspaceName: IndexingWorkspaceName);

        fixture.Rag.SearchAsync(Arg.Any<SufiAIRagSearchRequest>(), Arg.Any<CancellationToken>())

            .Returns(Task.FromException<SufiAIRagSearchResult>(

                new InvalidOperationException("Embedding provider returned HTTP 401")));

        var input = new HooshvareRuntimeRequestDto

        {

            HooshvareId = fixture.Definition.Id,

            Message = "What is Sufi Platform?",

            MetadataJson = HooshvareRequestContextMetadata.Merge(

                null,

                new Dictionary<string, string> { ["projectId"] = projectId.ToString("D") })

        };



        var result = await fixture.Orchestrator.PrepareRequestAsync(fixture.Definition, input);



        result.UsedRag.ShouldBeFalse();

        result.RetrievedChunkCount.ShouldBe(0);

        result.RagSearch.Outcome.ShouldBe(HooshvareRagSearchOutcome.Unavailable);

        result.Request.SystemPrompt!.ShouldContain(HooshvareRagRuntimeOptionsDefaults.RagUnavailableNotice);

        result.Request.SystemPrompt!.ShouldContain("RAG Indexing");

        result.Request.SystemPrompt!.ShouldContain("assign a hooshvare using a workspace whose Embeddings connection test succeeds");

        result.Request.SystemPrompt!.ShouldNotContain("Embedding provider returned HTTP 401");

        result.Request.SystemPrompt!.ShouldNotContain("Retrieved context:");

        await fixture.Circuit.Received(1).OpenAsync(

            IndexingWorkspaceName,

            nameof(InvalidOperationException),

            Arg.Any<CancellationToken>());

        await fixture.ProgressReporter.Received().ReportAsync(

            Arg.Is<HooshvareTurnProgressDto>(progress =>

                progress.Stage == HooshvareTurnProgressStages.SearchingKb

                && progress.Status == HooshvareTurnProgressStatuses.Failed),

            Arg.Any<CancellationToken>());

    }



    [Fact]

    public async Task Should_Skip_Rag_Search_When_Circuit_Is_Open()

    {

        var projectId = Guid.NewGuid();

        var fixture = CreateFixture(projectId, boundProjectIds: [projectId], indexingWorkspaceName: IndexingWorkspaceName);

        fixture.Circuit.IsOpenAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(true);

        var input = new HooshvareRuntimeRequestDto

        {

            HooshvareId = fixture.Definition.Id,

            Message = "What is Sufi Platform?",

            MetadataJson = HooshvareRequestContextMetadata.Merge(

                null,

                new Dictionary<string, string> { ["projectId"] = projectId.ToString("D") })

        };



        var result = await fixture.Orchestrator.PrepareRequestAsync(fixture.Definition, input);



        result.RagSearch.Outcome.ShouldBe(HooshvareRagSearchOutcome.SkippedCircuitOpen);

        result.Request.SystemPrompt!.ShouldContain(HooshvareRagRuntimeOptionsDefaults.RagUnavailableNotice);

        result.Request.SystemPrompt!.ShouldContain("RAG Indexing");

        result.Request.SystemPrompt!.ShouldContain("assign a hooshvare using a workspace whose Embeddings connection test succeeds");

        result.Request.SystemPrompt!.ShouldNotContain("Embedding provider returned HTTP 401");

        await fixture.Planner.DidNotReceive().DecideAsync(Arg.Any<HooshvareRagPlannerRequest>(), Arg.Any<CancellationToken>());

        await fixture.Rag.DidNotReceive().SearchAsync(Arg.Any<SufiAIRagSearchRequest>(), Arg.Any<CancellationToken>());

        await fixture.ProgressReporter.DidNotReceive().ReportAsync(

            Arg.Is<HooshvareTurnProgressDto>(progress => progress.Stage == HooshvareTurnProgressStages.SearchingKb),

            Arg.Any<CancellationToken>());

    }



    [Fact]

    public async Task Should_Skip_Rag_Search_When_Planner_Does_Not_Need_Knowledge()

    {

        var projectId = Guid.NewGuid();

        var fixture = CreateFixture(projectId, boundProjectIds: [projectId], indexingWorkspaceName: IndexingWorkspaceName);

        fixture.Planner.DecideAsync(Arg.Any<HooshvareRagPlannerRequest>(), Arg.Any<CancellationToken>())

            .Returns(new HooshvareRagPlannerDecision

            {

                SearchKb = false,

                Memory = "User greeted; no product question yet.",

                Succeeded = true

            });

        var input = new HooshvareRuntimeRequestDto

        {

            HooshvareId = fixture.Definition.Id,

            Message = "سلام",

            SessionSummary = "Earlier the user asked about licensing.",

            MetadataJson = HooshvareRequestContextMetadata.Merge(

                null,

                new Dictionary<string, string> { ["projectId"] = projectId.ToString("D") })

        };



        var result = await fixture.Orchestrator.PrepareRequestAsync(fixture.Definition, input);



        result.RagSearch.Outcome.ShouldBe(HooshvareRagSearchOutcome.SkippedByPlanner);

        result.UsedRag.ShouldBeFalse();

        result.SessionMemory.ShouldBe("User greeted; no product question yet.");

        result.SessionMemoryChanged.ShouldBeTrue();

        result.Request.Messages.ShouldContain(message =>

            message.Role == "system"

            && message.Content != null

            && message.Content.Contains("User greeted; no product question yet."));

        await fixture.Rag.DidNotReceive().SearchAsync(Arg.Any<SufiAIRagSearchRequest>(), Arg.Any<CancellationToken>());

    }



    [Fact]

    public async Task Should_Search_With_Planner_Query_When_Planner_Requests_Knowledge()

    {

        var projectId = Guid.NewGuid();

        var fixture = CreateFixture(projectId, boundProjectIds: [projectId], indexingWorkspaceName: IndexingWorkspaceName);

        fixture.Planner.DecideAsync(Arg.Any<HooshvareRagPlannerRequest>(), Arg.Any<CancellationToken>())

            .Returns(new HooshvareRagPlannerDecision

            {

                SearchKb = true,

                Query = "Sufi Platform license",

                Memory = "User wants licensing facts.",

                Succeeded = true

            });

        var input = new HooshvareRuntimeRequestDto

        {

            HooshvareId = fixture.Definition.Id,

            Message = "what about its license?",

            MetadataJson = HooshvareRequestContextMetadata.Merge(

                null,

                new Dictionary<string, string> { ["projectId"] = projectId.ToString("D") })

        };



        var result = await fixture.Orchestrator.PrepareRequestAsync(fixture.Definition, input);



        result.UsedRag.ShouldBeTrue();

        result.RagSearch.QueryMode.ShouldBe(HooshvareRagQueryMode.Planned);

        result.SessionMemory.ShouldBe("User wants licensing facts.");

        await fixture.Rag.Received().SearchAsync(

            Arg.Is<SufiAIRagSearchRequest>(request =>

                request.Query == "Sufi Platform license"

                && request.WorkspaceName == IndexingWorkspaceName),

            Arg.Any<CancellationToken>());

    }



    private static Fixture CreateFixture(Guid projectId, Guid[] boundProjectIds, string? indexingWorkspaceName)

    {

        var definition = new HooshvareDefinition(Guid.NewGuid(), null, "Tests", "HelpDesk assistant",

            HooshvareKind.Assistant, "Assistant", Guid.NewGuid(), "System instructions", false);

        definition.SetRuntimeOptions(new HooshvareRuntimeOptions

        {

            UseRag = true,

            RagSourceName = "KnowledgeBase",

            RagFilterByProjectId = true

        });



        var bindings = Substitute.For<IHooshvareRagProjectBindingRepository>();

        bindings.GetListByHooshvareAsync(definition.Id, "KnowledgeBase", Arg.Any<bool>(), Arg.Any<CancellationToken>())

            .Returns(boundProjectIds

                .Select(id => new HooshvareRagProjectBinding(Guid.NewGuid(), null, definition.Id, id, "KnowledgeBase"))

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



        var indexingWorkspaceResolver = Substitute.For<IHooshvareRagIndexingWorkspaceResolver>();

        indexingWorkspaceResolver.ResolveAsync(projectId, "KnowledgeBase", Arg.Any<CancellationToken>())

            .Returns(indexingWorkspaceName == null

                ? null

                : new HooshvareRagIndexingWorkspace

                {

                    ProjectId = projectId,

                    SourceName = "KnowledgeBase",

                    WorkspaceId = Guid.NewGuid(),

                    WorkspaceName = indexingWorkspaceName

                });



        var workspaceResolver = Substitute.For<IHooshvareWorkspaceResolver>();

        workspaceResolver.ResolveAsync(definition.WorkspaceId, Arg.Any<CancellationToken>())

            .Returns(new HooshvareWorkspaceBinding

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

        var progressReporter = Substitute.For<IHooshvareTurnProgressReporter>();

        var planner = Substitute.For<IHooshvareRagSearchPlanner>();

        planner.DecideAsync(Arg.Any<HooshvareRagPlannerRequest>(), Arg.Any<CancellationToken>())

            .Returns(call =>

            {

                var request = call.Arg<HooshvareRagPlannerRequest>();

                return new HooshvareRagPlannerDecision

                {

                    SearchKb = true,

                    Query = request.Input.Message,

                    Memory = request.PriorMemory,

                    Succeeded = true

                };

            });

        var circuit = Substitute.For<IHooshvareRagCircuitStore>();

        circuit.IsOpenAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);

        var retrieval = new HooshvareRagRetrievalService(

            rag,

            planner,

            circuit,

            progressReporter,

            NullLogger<HooshvareRagRetrievalService>.Instance);

        var orchestrator = new HooshvareRuntimeOrchestrator(

            retrieval,

            bindings,

            Substitute.For<IMCPToolRegistry>(),

            new HooshvareBusinessLocalizationService(localizers),

            localizers,

            workspaceResolver,

            runtimeResolver,

            new HooshvareContextTokenEstimator(),

            Substitute.For<IWorkspaceGuardrailService>(),

            indexingWorkspaceResolver,

            Substitute.For<IHooshvareContextFieldRegistry>(),

            NullLogger<HooshvareRuntimeOrchestrator>.Instance);



        return new Fixture(definition, rag, indexingWorkspaceResolver, orchestrator, progressReporter, planner, circuit);

    }



    private sealed record Fixture(

        HooshvareDefinition Definition,

        ISufiAIRagService Rag,

        IHooshvareRagIndexingWorkspaceResolver IndexingWorkspaceResolver,

        HooshvareRuntimeOrchestrator Orchestrator,

        IHooshvareTurnProgressReporter ProgressReporter,

        IHooshvareRagSearchPlanner Planner,

        IHooshvareRagCircuitStore Circuit);

}

