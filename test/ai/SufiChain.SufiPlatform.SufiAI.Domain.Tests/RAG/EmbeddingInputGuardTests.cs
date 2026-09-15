using Shouldly;
using SufiChain.SufiPlatform.SufiAI.RAG;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Domain.Tests.RAG;

public class EmbeddingInputGuardTests
{
    [Fact]
    public void Nvidia_budget_is_half_the_token_window()
    {
        EmbeddingInputGuard.GetCharacterBudget(EmbeddingModelDefaults.NvidiaEmbeddingMaxInputTokens)
            .ShouldBe(2048);
    }

    [Fact]
    public void Short_documents_keep_their_original_id()
    {
        var document = CreateDocument("article-1", "short body");

        var chunks = EmbeddingInputGuard.SplitDocuments(
            new[] { document },
            EmbeddingModelDefaults.NvidiaEmbeddingMaxInputTokens);

        chunks.Count.ShouldBe(1);
        chunks[0].Id.ShouldBe("article-1");
        chunks[0].SourceId.ShouldBe("article-1");
        chunks[0].Content.ShouldBe("short body");
        chunks[0].Metadata[EmbeddingInputGuard.ChunkCountMetadataKey].ShouldBe(1);
    }

    [Fact]
    public void Long_documents_are_split_under_the_character_budget()
    {
        var paragraph = new string('a', 1800);
        var document = CreateDocument("article-1", $"{paragraph}\n\n{paragraph}\n\n{paragraph}");
        var budget = EmbeddingInputGuard.GetCharacterBudget(4096);

        var chunks = EmbeddingInputGuard.SplitDocuments(
            new[] { document },
            maxInputTokens: 4096);

        chunks.Count.ShouldBeGreaterThan(1);
        chunks.ShouldAllBe(chunk => chunk.Content.Length <= budget);
        chunks.ShouldAllBe(chunk => chunk.SourceId == "article-1");
        chunks[0].Id.ShouldBe("article-1#0");
        chunks[^1].Id.ShouldBe($"article-1#{chunks.Count - 1}");
        chunks[0].Metadata[EmbeddingInputGuard.ChunkCountMetadataKey].ShouldBe(chunks.Count);
        chunks.Sum(chunk => chunk.Content.Contains('a') ? 1 : 0).ShouldBe(chunks.Count);
    }

    [Fact]
    public void FitToBudget_truncates_search_queries()
    {
        var budget = EmbeddingInputGuard.GetCharacterBudget(4096);
        var query = new string('q', budget + 50);

        var fitted = EmbeddingInputGuard.FitToBudget(query, 4096);

        fitted.Length.ShouldBe(budget);
    }

    private static DocumentChunk CreateDocument(string id, string content)
    {
        return new DocumentChunk
        {
            Id = id,
            SourceId = id,
            SourceName = "KnowledgeBase",
            Content = content,
            Metadata = new Dictionary<string, object>
            {
                ["articleId"] = id,
                ["title"] = "Sample"
            }
        };
    }
}
