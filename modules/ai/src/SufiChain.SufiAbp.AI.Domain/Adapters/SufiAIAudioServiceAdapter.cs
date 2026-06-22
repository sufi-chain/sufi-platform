using System.Threading;
using System.Threading.Tasks;
using SufiChain.SufiAbp.AI;
using SufiChain.SufiAbp.AI;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiAbp.AI.Adapters;

[Dependency(ReplaceServices = true)]
[ExposeServices(typeof(ISufiAIAudioService))]
public class SufiAIAudioServiceAdapter : ISufiAIAudioService, ITransientDependency
{
    protected IAIService AIService { get; }

    public SufiAIAudioServiceAdapter(IAIService aiService)
    {
        AIService = aiService;
    }

    public virtual Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }

    public virtual async Task<SufiAITranscriptionResponse> TranscribeAsync(
        SufiAITranscriptionRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await AIService.TranscribeAudioAsync(new AudioTranscriptionRequest
        {
            WorkspaceName = request.WorkspaceName,
            AudioData = request.AudioData,
            AudioFormat = request.AudioFormat,
            Language = request.Language,
            Prompt = request.Prompt
        }, cancellationToken);

        return new SufiAITranscriptionResponse
        {
            Text = response.Text,
            ModelId = response.ModelId,
            Language = response.Language,
            Usage = new SufiAITokenUsage
            {
                InputTokens = response.InputTokens,
                OutputTokens = response.OutputTokens,
                TotalTokens = response.TotalTokens,
                UnavailableReason = response.UsageUnavailableReason
            }
        };
    }
}
