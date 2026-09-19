namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// Model-route capabilities that a workspace can configure.
/// Chat, embeddings, vision, audio, image, and web routes are workspace-owned.
/// RAG authorization and MCP tool policy live on the hooshvare.
/// MCP servers are tenant-scoped, not workspace configuration.
/// </summary>
public enum AICapabilityType
{
    /// <summary>
    /// Text-based chat completion (e.g., GPT-4, Claude, Llama)
    /// </summary>
    ChatCompletion = 0,
    
    /// <summary>
    /// Audio transcription (speech-to-text, e.g., Whisper)
    /// </summary>
    AudioTranscription = 1,
    
    /// <summary>
    /// Text-to-speech synthesis (e.g., OpenAI TTS, Azure Speech)
    /// </summary>
    TextToSpeech = 2,
    
    /// <summary>
    /// Vision analysis (image understanding, e.g., GPT-4 Vision)
    /// </summary>
    VisionAnalysis = 3,
    
    /// <summary>
    /// Text embeddings for RAG and semantic search (e.g., text-embedding-3-small)
    /// </summary>
    Embeddings = 4,
    
    /// <summary>
    /// Image generation (e.g., DALL-E, Stable Diffusion)
    /// </summary>
    ImageGeneration = 5
    ,

    /// <summary>
    /// Search the web for current, external information.
    /// </summary>
    WebSearch = 6,

    /// <summary>
    /// Fetch and extract readable content from a web page.
    /// </summary>
    WebFetch = 7
}
