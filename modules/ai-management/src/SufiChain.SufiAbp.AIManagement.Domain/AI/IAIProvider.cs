using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SufiChain.SufiAbp.AIManagement.Workspaces;

namespace SufiChain.SufiAbp.AIManagement.AI;

/// <summary>
/// Provider abstraction for OpenAI-compatible AI services.
/// Each provider implements this interface to provide AI capabilities.
/// </summary>
public interface IAIProvider
{
    /// <summary>
    /// The provider type this implementation supports
    /// </summary>
    AIProviderType ProviderType { get; }
    
    /// <summary>
    /// Check if this provider supports a specific capability
    /// </summary>
    bool SupportsCapability(AICapabilityType capabilityType);
    
    /// <summary>
    /// Send a chat completion request
    /// </summary>
    Task<ChatCompletionResponse> SendChatMessageAsync(
        Workspace workspace,
        AIModelConfiguration configuration,
        ChatCompletionRequest request,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Send a chat completion request with streaming
    /// </summary>
    IAsyncEnumerable<ChatCompletionResponse> StreamChatMessageAsync(
        Workspace workspace,
        AIModelConfiguration configuration,
        ChatCompletionRequest request,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Transcribe audio to text
    /// </summary>
    Task<AudioTranscriptionResponse> TranscribeAudioAsync(
        Workspace workspace,
        AIModelConfiguration configuration,
        AudioTranscriptionRequest request,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Convert text to speech
    /// </summary>
    Task<TextToSpeechResponse> GenerateSpeechAsync(
        Workspace workspace,
        AIModelConfiguration configuration,
        TextToSpeechRequest request,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Analyze an image
    /// </summary>
    Task<VisionAnalysisResponse> AnalyzeImageAsync(
        Workspace workspace,
        AIModelConfiguration configuration,
        VisionAnalysisRequest request,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Generate text embeddings
    /// </summary>
    Task<EmbeddingsResponse> GenerateEmbeddingsAsync(
        Workspace workspace,
        AIModelConfiguration configuration,
        EmbeddingsRequest request,
        CancellationToken cancellationToken = default);
}
