using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.RAG;
using SufiChain.SufiPlatform.SufiAI;
using Xunit;

namespace SufiChain.SufiPlatform.HelpDesk.KnowledgeBase;

/// <summary>
/// "Indexed" must mean vectors for this exact article exist and are reachable through the same
/// metadata-filter path the runtime search uses.
/// </summary>
public class KBArticleRagIndexerTests
{
    private const string WorkspaceName = "helpdesk-rag";

    [Fact]
    public async Task Should_Not_Report_Indexed_When_Zero_Chunks_Were_Stored()
    {
        var articleId = Guid.NewGuid();
        var rag = Substitute.For<ISufiAIRagService>();
        rag.IndexDocumentAsync(Arg.Any<SufiAIRagIndexDocumentRequest>(), Arg.Any<CancellationToken>())
            .Returns(new SufiAIRagIndexDocumentResult { DocumentId = articleId.ToString("D"), StoredChunkCount = 0 });
        var indexer = new KBArticleRagIndexer(rag);

        var result = await indexer.IndexArticleAsync(WorkspaceName, articleId);

        result.IsIndexed.ShouldBeFalse();
        result.StoredChunkCount.ShouldBe(0);
        result.RetrievableChunkCount.ShouldBe(0);
        await rag.DidNotReceive().CountAsync(Arg.Any<SufiAIRagCountRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Not_Report_Indexed_When_Stored_Chunks_Are_Not_Retrievable_By_ArticleId_Filter()
    {
        var articleId = Guid.NewGuid();
        var rag = Substitute.For<ISufiAIRagService>();
        rag.IndexDocumentAsync(Arg.Any<SufiAIRagIndexDocumentRequest>(), Arg.Any<CancellationToken>())
            .Returns(new SufiAIRagIndexDocumentResult { DocumentId = articleId.ToString("D"), StoredChunkCount = 3 });
        rag.CountAsync(Arg.Any<SufiAIRagCountRequest>(), Arg.Any<CancellationToken>()).Returns(0);
        var indexer = new KBArticleRagIndexer(rag);

        var result = await indexer.IndexArticleAsync(WorkspaceName, articleId);

        result.IsIndexed.ShouldBeFalse();
        result.StoredChunkCount.ShouldBe(3);
        result.RetrievableChunkCount.ShouldBe(0);
    }

    [Fact]
    public async Task Should_Report_Indexed_When_Chunks_For_The_Article_Are_Retrievable()
    {
        var articleId = Guid.NewGuid();
        var rag = Substitute.For<ISufiAIRagService>();
        rag.IndexDocumentAsync(
                Arg.Is<SufiAIRagIndexDocumentRequest>(request =>
                    request.WorkspaceName == WorkspaceName &&
                    request.SourceName == KBArticleDocumentSource.Name &&
                    request.DocumentId == articleId.ToString("D")),
                Arg.Any<CancellationToken>())
            .Returns(new SufiAIRagIndexDocumentResult { DocumentId = articleId.ToString("D"), StoredChunkCount = 2 });
        rag.CountAsync(
                Arg.Is<SufiAIRagCountRequest>(request =>
                    request.WorkspaceName == WorkspaceName &&
                    request.SourceName == KBArticleDocumentSource.Name &&
                    request.MetadataFilters[KBArticleRagIndexer.ArticleIdMetadataKey] == articleId.ToString("D")),
                Arg.Any<CancellationToken>())
            .Returns(2);
        var indexer = new KBArticleRagIndexer(rag);

        var result = await indexer.IndexArticleAsync(WorkspaceName, articleId);

        result.IsIndexed.ShouldBeTrue();
        result.RetrievableChunkCount.ShouldBe(2);
    }
}
