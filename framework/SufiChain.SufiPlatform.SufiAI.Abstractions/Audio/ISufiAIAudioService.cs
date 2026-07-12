using System.Threading;
using System.Threading.Tasks;

namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// Platform-level audio transcription service for product modules.
/// Stable public contract; a provider module (e.g. AI) replaces the Null
/// default. Provider SDK types are never exposed here.
/// </summary>
public interface ISufiAIAudioService
{
    /// <summary>
    /// Whether a real AI provider implementation is installed and usable.
    /// Returns <c>false</c> for the Null fallback so modules can degrade gracefully.
    /// </summary>
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Transcribes audio to text (speech-to-text).
    /// </summary>
    Task<SufiAITranscriptionResponse> TranscribeAsync(
        SufiAITranscriptionRequest request,
        CancellationToken cancellationToken = default);
}
