using Microsoft.Extensions.Localization;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.AI;
using SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.Articles;
using SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.Localization;
using SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.RAG;
using SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.Repositories;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.Copilots.Copilots;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Timing;
using Xunit;

namespace SufiChain.SufiPlatform.HelpDesk.KnowledgeBase;

/// <summary>
/// The smoke test must fail closed: "ready" requires stored vectors for the exact article AND a
/// citation of that article coming back through the projectId-filtered search.
/// </summary>
public class KBRagSmokeTestAppServiceTests
{
    private const string WorkspaceName = "helpdesk-rag";

    [Fact]
    public async Task Should_Fail_Closed_When_No_Chunks_Are_Stored_For_The_Article()
    {
        var fixture = Fixture.Create(storedChunkCount: 0, retrievableChunkCount: 0, searchHits: []);

        var result = await fixture.Service.RunAsync(fixture.ProjectId, fixture.Article.Id, string.Empty);

        result.IsReady.ShouldBeFalse();
        result.ArticleIndexed.ShouldBeFalse();
        result.CitationReturned.ShouldBeFalse();
        result.FailureReason.ShouldBe("KnowledgeBase:RagArticleChunksNotStored");
        fixture.Article.IsIndexed.ShouldBeFalse();
        await fixture.Rag.DidNotReceive().SearchAsync(Arg.Any<SufiAIRagSearchRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Fail_Closed_When_Chunks_Exist_But_Search_Does_Not_Cite_The_Article()
    {
        var fixture = Fixture.Create(
            storedChunkCount: 1,
            retrievableChunkCount: 1,
            searchHits:
            [
                new SufiAIDocumentChunk
                {
                    SourceName = KBArticleDocumentSource.Name,
                    SourceId = Guid.NewGuid().ToString("D"),
                    Content = "Some other article",
                    Score = 0.8f
                }
            ]);

        var result = await fixture.Service.RunAsync(fixture.ProjectId, fixture.Article.Id, string.Empty);

        result.ArticleIndexed.ShouldBeTrue();
        result.HitCount.ShouldBe(1);
        result.CitationReturned.ShouldBeFalse();
        result.IsReady.ShouldBeFalse();
        result.FailureReason.ShouldBe("KnowledgeBase:RagCitationNotReturned");
    }

    [Fact]
    public async Task Should_Pass_With_Diagnostics_When_Chunk_For_The_Article_Is_Cited()
    {
        var articleId = Guid.NewGuid();
        var fixture = Fixture.Create(
            storedChunkCount: 1,
            retrievableChunkCount: 1,
            searchHits:
            [
                new SufiAIDocumentChunk
                {
                    SourceName = KBArticleDocumentSource.Name,
                    SourceId = articleId.ToString("D"),
                    Content = "Reset your password from the profile page.",
                    Score = 0.42f
                }
            ],
            articleId: articleId);

        var result = await fixture.Service.RunAsync(fixture.ProjectId, articleId, string.Empty);

        result.IsReady.ShouldBeTrue();
        result.ArticleIndexed.ShouldBeTrue();
        result.CitationReturned.ShouldBeTrue();
        result.WorkspaceName.ShouldBe(WorkspaceName);
        result.StoredChunkCount.ShouldBe(1);
        result.RetrievableChunkCount.ShouldBe(1);
        result.HitCount.ShouldBe(1);
        result.TopScore.ShouldBe(0.42f);
        result.ArticleScore.ShouldBe(0.42f);
        result.MinSimilarityUsed.ShouldBe(KBRagSmokeTestAppService.SmokeTestMinSimilarity);
        result.MeetsDefaultSimilarity.ShouldBeFalse();
        result.Warning.ShouldBe("KnowledgeBase:RagCitationBelowDefaultSimilarity");
        result.FailureReason.ShouldBeNull();
        result.Query.ShouldContain("Reset password");
        fixture.Article.IsIndexed.ShouldBeTrue();

        await fixture.Rag.Received(1).SearchAsync(
            Arg.Is<SufiAIRagSearchRequest>(request =>
                request.WorkspaceName == WorkspaceName &&
                request.SourceName == KBArticleDocumentSource.Name &&
                request.MinSimilarity == KBRagSmokeTestAppService.SmokeTestMinSimilarity &&
                request.MetadataFilters["projectId"] == fixture.ProjectId.ToString("D")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Smoke_Threshold_Should_Be_Below_Runtime_Default()
    {
        KBRagSmokeTestAppService.SmokeTestMinSimilarity.ShouldBeLessThan(KBRagSmokeTestAppService.DefaultMinSimilarity);
        KBRagSmokeTestAppService.DefaultMinSimilarity.ShouldBe(0.35f);
    }

    private sealed class Fixture
    {
        public required Guid ProjectId { get; init; }
        public required KBArticle Article { get; init; }
        public required ISufiAIRagService Rag { get; init; }
        public required TestableKBRagSmokeTestAppService Service { get; init; }

        public static Fixture Create(
            int storedChunkCount,
            int retrievableChunkCount,
            List<SufiAIDocumentChunk> searchHits,
            Guid? articleId = null)
        {
            var projectId = Guid.NewGuid();
            var article = new KBArticle(articleId ?? Guid.NewGuid(), projectId, Guid.NewGuid());
            article.AddTranslation(Guid.NewGuid(), "en", "Reset password", "reset-password",
                "Reset your password from the profile page.");
            article.Publish(DateTime.UtcNow);

            var articles = Substitute.For<IKBArticleRepository>();
            articles.GetAsync(article.Id, Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(article);
            articles.UpdateAsync(Arg.Any<KBArticle>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
                .Returns(call => call.Arg<KBArticle>());

            var workspaceId = Guid.NewGuid();
            var workspaceResolver = Substitute.For<IKBProjectAIWorkspaceResolver>();
            workspaceResolver.ResolveRagWorkspaceAsync(projectId, Arg.Any<CancellationToken>())
                .Returns(new KBResolvedAIWorkspace
                {
                    WorkspaceId = workspaceId,
                    CopilotId = Guid.NewGuid(),
                    CopilotName = "KB Editor",
                    WorkspaceName = WorkspaceName,
                    Purpose = "RagIndexing"
                });

            var catalog = Substitute.For<ISufiAIWorkspaceCatalog>();
            catalog.FindByIdAsync(workspaceId, Arg.Any<CancellationToken>())
                .Returns(new SufiAIWorkspaceDescriptor { Id = workspaceId, Name = WorkspaceName, IsReady = true });

            var rag = Substitute.For<ISufiAIRagService>();
            rag.IsAvailableAsync(Arg.Any<CancellationToken>()).Returns(true);
            rag.IndexDocumentAsync(Arg.Any<SufiAIRagIndexDocumentRequest>(), Arg.Any<CancellationToken>())
                .Returns(new SufiAIRagIndexDocumentResult
                {
                    DocumentId = article.Id.ToString("D"),
                    StoredChunkCount = storedChunkCount
                });
            rag.CountAsync(Arg.Any<SufiAIRagCountRequest>(), Arg.Any<CancellationToken>())
                .Returns(retrievableChunkCount);
            rag.SearchAsync(Arg.Any<SufiAIRagSearchRequest>(), Arg.Any<CancellationToken>())
                .Returns(new SufiAIRagSearchResult { Chunks = searchHits });

            var ragCircuit = Substitute.For<ICopilotRagCircuitStore>();
            var service = new TestableKBRagSmokeTestAppService(
                articles,
                workspaceResolver,
                rag,
                catalog,
                new KBArticleRagIndexer(rag),
                ragCircuit);

            return new Fixture { ProjectId = projectId, Article = article, Rag = rag, Service = service };
        }
    }

    /// <summary>
    /// Bypasses the ABP DI container: localization returns the key so assertions stay culture-independent.
    /// </summary>
    private sealed class TestableKBRagSmokeTestAppService : KBRagSmokeTestAppService
    {
        public TestableKBRagSmokeTestAppService(
            IKBArticleRepository articleRepository,
            IKBProjectAIWorkspaceResolver workspaceResolver,
            ISufiAIRagService ragService,
            ISufiAIWorkspaceCatalog workspaceCatalog,
            KBArticleRagIndexer articleIndexer,
            ICopilotRagCircuitStore ragCircuit)
            : base(articleRepository, workspaceResolver, ragService, workspaceCatalog, articleIndexer, ragCircuit)
        {
            LocalizationResource = typeof(KnowledgeBaseResource);

            var localizer = Substitute.For<IStringLocalizer>();
            localizer[Arg.Any<string>()].Returns(call => new LocalizedString(call.Arg<string>(), call.Arg<string>()));
            var factory = Substitute.For<IStringLocalizerFactory>();
            factory.Create(Arg.Any<Type>()).Returns(localizer);

            var clock = Substitute.For<IClock>();
            clock.Now.Returns(DateTime.UtcNow);

            var lazy = Substitute.For<IAbpLazyServiceProvider>();
            lazy.LazyGetRequiredService<IStringLocalizerFactory>().Returns(factory);
            lazy.LazyGetRequiredService<IClock>().Returns(clock);
            LazyServiceProvider = lazy;
        }
    }
}
