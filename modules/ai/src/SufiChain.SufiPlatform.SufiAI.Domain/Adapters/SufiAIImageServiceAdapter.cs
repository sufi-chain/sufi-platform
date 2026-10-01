using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.SufiAI.Adapters;

[Dependency(ReplaceServices = true)]
[ExposeServices(typeof(ISufiAIImageService))]
public class SufiAIImageServiceAdapter : ISufiAIImageService, ITransientDependency
{
    protected IAIService AIService { get; }

    public SufiAIImageServiceAdapter(IAIService aiService)
    {
        AIService = aiService;
    }

    public virtual Task<bool> IsAvailableAsync(string workspaceName, CancellationToken cancellationToken = default)
    {
        return AIService.HasCapabilityAsync(workspaceName, AICapabilityType.ImageGeneration, cancellationToken);
    }

    public virtual async Task<SufiAIImageGenerationResponse> GenerateAsync(
        SufiAIImageGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await AIService.GenerateImageAsync(new ImageGenerationRequest
        {
            WorkspaceName = request.WorkspaceName,
            Prompt = request.Prompt,
            Size = request.Size,
            Quality = request.Quality,
            OutputFormat = request.OutputFormat
        }, cancellationToken);

        return new SufiAIImageGenerationResponse
        {
            ImageData = response.ImageData,
            MimeType = response.MimeType,
            RevisedPrompt = response.RevisedPrompt,
            ModelId = response.ModelId
        };
    }
}
