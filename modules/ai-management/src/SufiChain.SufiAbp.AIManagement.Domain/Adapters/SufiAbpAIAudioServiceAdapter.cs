using System.Threading;
using System.Threading.Tasks;
using SufiChain.SufiAbp.AI;
using SufiChain.SufiAbp.AIManagement.AI;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiAbp.AIManagement.Adapters;

[Dependency(ReplaceServices = true)]
[ExposeServices(typeof(ISufiAbpAIAudioService))]
public class SufiAbpAIAudioServiceAdapter : ISufiAbpAIAudioService, ITransientDependency
{
    protected IAIService AIService { get; }

    public SufiAbpAIAudioServiceAdapter(IAIService aiService)
    {
        AIService = aiService;
    }

    public virtual Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }

    public virtual async Task<SufiAbpAITranscriptionResponse> TranscribeAsync(
        SufiAbpAITranscriptionRequest request,
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

        return new SufiAbpAITranscriptionResponse
        {
            Text = response.Text,
            ModelId = response.ModelId,
            Language = response.Language,
            Usage = new SufiAbpAITokenUsage
            {
                InputTokens = response.InputTokens,
                OutputTokens = response.OutputTokens,
                TotalTokens = response.TotalTokens,
                UnavailableReason = response.UsageUnavailableReason
            }
        };
    }
}
