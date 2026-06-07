using SufiChain.SufiAbp.AIManagement.AI;
using Volo.Abp.DependencyInjection;

namespace SufiChain.Chat.Supports;

public class ConfigurableAiService : IAIService, ISingletonDependency
{
    public string ResponseContent { get; set; } = "Suggested operator reply";

    public Task<ChatCompletionResponse> SendChatMessageAsync(
        ChatCompletionRequest request,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new ChatCompletionResponse
        {
            Content = ResponseContent,
            ModelId = "test-model",
            InputTokens = 5,
            OutputTokens = 10,
            TotalTokens = 15
        });
    }

    public IAsyncEnumerable<ChatCompletionResponse> StreamChatMessageAsync(
        ChatCompletionRequest request,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public Task<AudioTranscriptionResponse> TranscribeAudioAsync(
        AudioTranscriptionRequest request,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public Task<TextToSpeechResponse> GenerateSpeechAsync(
        TextToSpeechRequest request,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public Task<VisionAnalysisResponse> AnalyzeImageAsync(
        VisionAnalysisRequest request,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public Task<EmbeddingsResponse> GenerateEmbeddingsAsync(
        EmbeddingsRequest request,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public Task<bool> HasCapabilityAsync(
        string workspaceName,
        AICapabilityType capabilityType,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }
}
