using Shouldly;

using SufiChain.SufiPlatform.SufiAI;

using Xunit;



namespace SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;



public class HooshvareRuntimeCitationMappingTests

{

    [Fact]

    public void MapCitations_Should_Cap_At_RagTopK()

    {

        var chunks = Enumerable.Range(0, 6)

            .Select(i => new SufiAIDocumentChunk

            {

                Id = $"chunk-{i}",

                SourceId = $"article-{i}",

                SourceName = "KnowledgeBase",

                Content = $"Body {i}",

                Score = 1f - (i * 0.1f),

                Metadata = new Dictionary<string, object> { ["title"] = $"Title {i}" }

            })

            .ToList();



        var citations = HooshvareRuntimeOrchestrator.MapCitations(chunks, ragTopK: 3);



        citations.Count.ShouldBe(3);

        citations[0].Title.ShouldBe("Title 0");

        citations[0].DocumentId.ShouldBe("article-0");

        citations[2].Title.ShouldBe("Title 2");

    }



    [Fact]

    public void MapCitations_Should_Truncate_Snippet()

    {

        var chunks = new List<SufiAIDocumentChunk>

        {

            new()

            {

                Id = "c1",

                SourceId = "doc-1",

                SourceName = "KnowledgeBase",

                Content = new string('a', 500),

                Score = 0.9f

            }

        };



        var citations = HooshvareRuntimeOrchestrator.MapCitations(chunks, ragTopK: 5, snippetLength: 400);



        citations.Single().Snippet.ShouldNotBeNull();

        citations.Single().Snippet!.EndsWith("...", StringComparison.Ordinal).ShouldBeTrue();

        citations.Single().Snippet!.Length.ShouldBe(403);

    }

}

