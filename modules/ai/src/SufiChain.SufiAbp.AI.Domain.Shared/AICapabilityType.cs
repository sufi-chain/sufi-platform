namespace SufiChain.SufiAbp.AI;

/// <summary>
/// Defines the types of AI capabilities that can be configured per workspace.
/// Each capability type may require different models, endpoints, and configuration.
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
}
