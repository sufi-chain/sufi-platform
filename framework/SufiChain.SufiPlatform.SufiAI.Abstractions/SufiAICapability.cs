namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// Provider-neutral AI capability flags exposed to product modules.
/// Replaces provider/admin-specific capability enums in product-facing contracts.
/// </summary>
public enum SufiAICapability
{
    /// <summary>
    /// Text chat completion.
    /// </summary>
    Chat = 0,

    /// <summary>
    /// Streaming chat completion.
    /// </summary>
    Streaming = 1,

    /// <summary>
    /// Audio transcription (speech-to-text) and speech generation.
    /// </summary>
    Audio = 2,

    /// <summary>
    /// Vision/image analysis.
    /// </summary>
    Vision = 3,

    /// <summary>
    /// Embedding generation for semantic search and RAG.
    /// </summary>
    Embeddings = 4,

    /// <summary>
    /// LLM-callable tool/function execution.
    /// </summary>
    Tools = 5
}
