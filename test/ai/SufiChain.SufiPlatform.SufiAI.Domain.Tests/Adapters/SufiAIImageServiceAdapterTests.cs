using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.Adapters;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Domain.Tests.Adapters;

public class SufiAIImageServiceAdapterTests
{
    [Fact]
    public async Task GenerateAsync_Maps_Prompt_And_Bytes_From_IAIService()
    {
        var aiService = Substitute.For<IAIService>();
        ImageGenerationRequest? mapped = null;
        aiService.GenerateImageAsync(Arg.Any<ImageGenerationRequest>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                mapped = call.Arg<ImageGenerationRequest>();
                return new ImageGenerationResponse
                {
                    ImageData = [9, 8, 7],
                    MimeType = "image/webp",
                    RevisedPrompt = "a lamp on a table",
                    ModelId = "img-1"
                };
            });

        var adapter = new SufiAIImageServiceAdapter(aiService);
        var result = await adapter.GenerateAsync(new SufiAIImageGenerationRequest
        {
            WorkspaceName = "cms",
            Prompt = "a lamp",
            Size = "1024x1024",
            OutputFormat = "webp"
        });

        mapped.ShouldNotBeNull();
        mapped!.WorkspaceName.ShouldBe("cms");
        mapped.Prompt.ShouldBe("a lamp");
        mapped.OutputFormat.ShouldBe("webp");
        result.ImageData.ShouldBe([9, 8, 7]);
        result.MimeType.ShouldBe("image/webp");
        result.RevisedPrompt.ShouldBe("a lamp on a table");
    }
}
