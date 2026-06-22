namespace SufiChain.SufiAbp.AI;

/// <summary>
/// Result of an audio transcription.
/// </summary>
public class SufiAITranscriptionResponse
{
    /// <summary>
    /// Transcribed text.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Identifier of the model that produced the transcription.
    /// </summary>
    public string ModelId { get; set; } = string.Empty;

    /// <summary>
    /// Detected or requested language, when available.
    /// </summary>
    public string? Language { get; set; }

    /// <summary>
    /// Token usage for the request, when reported by the provider.
    /// </summary>
    public SufiAITokenUsage Usage { get; set; } = new();
}
