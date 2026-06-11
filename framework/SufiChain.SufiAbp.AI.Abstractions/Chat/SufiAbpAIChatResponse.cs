namespace SufiChain.SufiAbp.AI;

/// <summary>
/// Response of a non-streaming chat completion.
/// </summary>
public class SufiAbpAIChatResponse
{
    /// <summary>
    /// Generated message content.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Identifier of the model that produced the response.
    /// </summary>
    public string ModelId { get; set; } = string.Empty;

    /// <summary>
    /// Token usage for the request, when reported by the provider.
    /// </summary>
    public SufiAbpAITokenUsage Usage { get; set; } = new();

    /// <summary>
    /// Provider finish reason (e.g. <c>stop</c>, <c>length</c>), when available.
    /// </summary>
    public string? FinishReason { get; set; }
}
