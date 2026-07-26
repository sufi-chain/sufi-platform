using Shouldly;
using SufiChain.SufiPlatform.SufiAI.RAG;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Domain.Tests.RAG;

public class DocumentChunkMetadataFilterTests
{
    [Fact]
    public void Matches_Should_Require_All_Filter_Keys()
    {
        var chunk = new DocumentChunk
        {
            Id = Guid.NewGuid().ToString("D"),
            Metadata = new Dictionary<string, object>
            {
                ["projectId"] = Guid.Parse("11111111-1111-1111-1111-111111111111").ToString("D"),
                ["articleId"] = "article-1"
            }
        };

        DocumentChunkMetadataFilter.Matches(
                chunk,
                new Dictionary<string, string> { ["projectId"] = "11111111-1111-1111-1111-111111111111" })
            .ShouldBeTrue();

        DocumentChunkMetadataFilter.Matches(
                chunk,
                new Dictionary<string, string>
                {
                    ["projectId"] = "11111111-1111-1111-1111-111111111111",
                    ["categoryId"] = "missing"
                })
            .ShouldBeFalse();
    }

    [Fact]
    public void Filter_Should_Keep_Only_Matching_Chunks()
    {
        var projectA = Guid.NewGuid().ToString("D");
        var projectB = Guid.NewGuid().ToString("D");
        var documents = new List<DocumentChunk>
        {
            new() { Id = "1", Metadata = new Dictionary<string, object> { ["projectId"] = projectA } },
            new() { Id = "2", Metadata = new Dictionary<string, object> { ["projectId"] = projectB } },
            new() { Id = "3", Metadata = new Dictionary<string, object> { ["projectId"] = projectA } }
        };

        var filtered = DocumentChunkMetadataFilter.Filter(
            documents,
            new Dictionary<string, string> { ["projectId"] = projectA });

        filtered.Select(x => x.Id).ShouldBe(["1", "3"]);
    }

    [Fact]
    public void Filter_Should_Return_All_When_Filters_Empty()
    {
        var documents = new List<DocumentChunk>
        {
            new() { Id = "1" },
            new() { Id = "2" }
        };

        DocumentChunkMetadataFilter.Filter(documents, new Dictionary<string, string>())
            .Count.ShouldBe(2);
    }
}
