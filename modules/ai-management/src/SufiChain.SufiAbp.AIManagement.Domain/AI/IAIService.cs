using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SufiChain.SufiAbp.AIManagement.AI;

/// <summary>
/// Unified AI service interface supporting multiple capabilities (chat, audio, vision, embeddings).
/// This is the main entry point for all AI operations in the system.
/// </summary>
public interface IAIService
{
    /// <summary>
    /// Send a chat completion request (text-based conversation)
    /// </summary>
    Task<ChatCompletionResponse> SendChatMessageAsync(
        ChatCompletionRequest request,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Send a chat completion request with streaming response
    /// </summary>
    IAsyncEnumerable<ChatCompletionResponse> StreamChatMessageAsync(
        ChatCompletionRequest request,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Transcribe audio to text (speech-to-text)
    /// </summary>
    Task<AudioTranscriptionResponse> TranscribeAudioAsync(
        AudioTranscriptionRequest request,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Convert text to speech (text-to-speech)
    /// </summary>
    Task<TextToSpeechResponse> GenerateSpeechAsync(
        TextToSpeechRequest request,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Analyze an image with AI vision (image understanding)
    /// </summary>
    Task<VisionAnalysisResponse> AnalyzeImageAsync(
        VisionAnalysisRequest request,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Generate text embeddings for semantic search and RAG
    /// </summary>
    Task<EmbeddingsResponse> GenerateEmbeddingsAsync(
        EmbeddingsRequest request,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Check if a workspace has a specific capability enabled
    /// </summary>
    Task<bool> HasCapabilityAsync(
        string workspaceName,
        AICapabilityType capabilityType,
        CancellationToken cancellationToken = default);
}
