using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;

public class HooshvareKnowledgeLibraryRetrievalTests
{
    [Fact]
    public async Task Hooshvare_Source_Should_Filter_By_Hooshvare_Normalize_Query_And_Expand_By_File()
    {
        var hooshvareId = Guid.NewGuid();
        var fileId = Guid.NewGuid().ToString("D");
        var requests = new List<SufiAIRagSearchRequest>();
        var rag = Substitute.For<ISufiAIRagService>();
        rag.SearchAsync(Arg.Any<SufiAIRagSearchRequest>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                requests.Add(call.Arg<SufiAIRagSearchRequest>());
                return new SufiAIRagSearchResult
                {
                    Chunks =
                    [
                        new SufiAIDocumentChunk
                        {
                            Id = fileId + ":0",
                            SourceId = fileId,
                            SourceName = HooshvareRagSourceNames.HooshvareKnowledgeBase,
                            Content = "passage",
                            Score = 0.9f,
                            Metadata = new Dictionary<string, object>
                            {
                                ["fileId"] = fileId,
                                ["hooshvareId"] = hooshvareId.ToString("D")
                            }
                        }
                    ]
                };
            });

        var retrieval = HooshvareRuntimeTestSupport.CreateRagRetrieval(rag, searchKb: true);

        var result = await retrieval.RetrieveAsync(new HooshvareRagRetrievalRequest
        {
            HooshvareId = hooshvareId,
            Input = new HooshvareRuntimeRequestDto { Message = "قيمت كالا چنده؟" },
            Options = new HooshvareRuntimeOptions
            {
                UseRag = true,
                RagSourceName = HooshvareRagSourceNames.HooshvareKnowledgeBase,
                RagFilterByProjectId = true,
                RagMetadataFilters = new Dictionary<string, string> { ["projectId"] = Guid.NewGuid().ToString("D") }
            },
            ChatWorkspaceName = "chat",
            RagWorkspaceName = "library"
        });

        result.Chunks.Count.ShouldBe(1);
        requests.Count.ShouldBe(2);
        requests.ShouldAllBe(request =>
            request.WorkspaceName == "library"
            && request.SourceName == HooshvareRagSourceNames.HooshvareKnowledgeBase
            && request.MetadataFilters["hooshvareId"] == hooshvareId.ToString("D")
            && !request.MetadataFilters.ContainsKey("projectId"));
        requests[0].Query.ShouldBe("قیمت کالا چنده؟");
        requests[1].MetadataFilters["fileId"].ShouldBe(fileId);
        requests[1].MetadataFilters.ContainsKey("articleId").ShouldBeFalse();
    }
}
