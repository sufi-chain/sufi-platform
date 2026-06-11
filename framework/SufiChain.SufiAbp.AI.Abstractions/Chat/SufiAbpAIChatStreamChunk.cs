namespace SufiChain.SufiAbp.AI;

/// <summary>
/// A single chunk of a streaming chat completion.
/// Content chunks carry incremental text; the final chunk carries token usage
/// (when available) and the finish reason. Consumers must record usage once per
/// completed stream, not per chunk.
/// </summary>
public class SufiAbpAIChatStreamChunk
{
    /// <summary>
    /// Incremental content delta. May be empty on the final/usage chunk.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Identifier of the model producing the stream, when known.
    /// </summary>
    public string ModelId { get; set; } = string.Empty;

    /// <summary>
    /// Token usage for the whole request. Only populated on the final chunk,
    /// when reported by the provider.
    /// </summary>
    public SufiAbpAITokenUsage? Usage { get; set; }

    /// <summary>
    /// Whether this is the final chunk of the stream.
    /// </summary>
    public bool IsFinal { get; set; }

    /// <summary>
    /// Provider finish reason, populated on the final chunk when available.
    /// </summary>
    public string? FinishReason { get; set; }
}
