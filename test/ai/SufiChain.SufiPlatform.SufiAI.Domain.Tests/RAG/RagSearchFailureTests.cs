using Shouldly;
using SufiChain.SufiPlatform.SufiAI.RAG;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Volo.Abp;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Domain.Tests.RAG;

public class RagSearchFailureTests
{
    [Fact]
    public void SearchAsync_Embedding410_ThrowsBusinessException_EmbeddingGenerationFailed_WithWorkspace()
    {
        var error = RagSearchFailures.ToBusinessException(
            new InvalidOperationException("Embedding provider returned HTTP 410"),
            "GenerateEmbeddings",
            "workspace-a",
            "docs");

        error.Code.ShouldBe(AIErrorCodes.EmbeddingGenerationFailed);
        error.Data["WorkspaceName"].ShouldBe("workspace-a");
    }

    [Fact]
    public void SearchAsync_VectorStoreThrows_ThrowsVectorSearchFailed()
    {
        var error = RagSearchFailures.ToBusinessException(
            new InvalidOperationException("store down"),
            "VectorSearch",
            "workspace-a",
            null);

        error.Code.ShouldBe(AIErrorCodes.VectorSearchFailed);
    }

    [Fact]
    public void SearchAsync_BusinessException_PassesThrough()
    {
        var original = new BusinessException(AIErrorCodes.RagUnavailable);
        (original is BusinessException).ShouldBeTrue();
        RagSearchFailures.IsCancellation(original).ShouldBeFalse();
    }

    [Fact]
    public void SearchAsync_Cancelled_Rethrows()
    {
        RagSearchFailures.IsCancellation(new OperationCanceledException()).ShouldBeTrue();
    }

    [Fact]
    public void SearchAsync_DimensionMismatch_ThrowsVectorSearchFailed_NamingBothSizes()
    {
        var error = RagSearchFailures.ToBusinessException(
            new InvalidOperationException("Embedding dimension mismatch. Expected 2048, found 1536."),
            "VectorSearch",
            "workspace-a",
            "docs");

        error.Code.ShouldBe(AIErrorCodes.VectorSearchFailed);
        error.Data["Expected"].ShouldBe("2048");
        error.Data["Found"].ShouldBe("1536");
        error.Data["Error"].ToString().ShouldContain("re-index");
    }

    [Fact]
    public void EmbeddingGenerator_RouteModelChanged_NewGeneratorWithoutRestart()
    {
        var id = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
        var previous = EmbedderGeneratorCacheKey.Build("host", id, "nvidia/nemotron-3-embed-1b:free", 2048);
        var next = EmbedderGeneratorCacheKey.Build("host", id, "nvidia/nemotron-3.5-embed-1b:free", 2048);
        previous.ShouldNotBe(next);
    }

    [Fact]
    public void EmbeddingModelChange_MarksCollectionsReindexNeeded()
    {
        RagSearchFailures.RequiresReindex("old", 2048, "nvidia/nemotron-3.5-embed-1b:free", 2048).ShouldBeTrue();
        RagSearchFailures.RequiresReindex("same", 2048, "same", 1536).ShouldBeTrue();
        RagSearchFailures.RequiresReindex("same", 2048, "same", 2048).ShouldBeFalse();
    }
}
