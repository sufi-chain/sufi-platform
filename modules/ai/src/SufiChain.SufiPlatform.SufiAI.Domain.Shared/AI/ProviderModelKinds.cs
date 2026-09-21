using System;

namespace SufiChain.SufiPlatform.SufiAI;

[Flags]
public enum ProviderModelKinds
{
    None = 0,
    Chat = 1,
    Embeddings = 2,
    AudioTranscription = 4,
    TextToSpeech = 8,
    ImageGeneration = 16,
    Vision = 32
}
