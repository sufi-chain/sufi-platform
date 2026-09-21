using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Domain.Tests.AI;

public class ProviderModelCapabilityClassifierTests
{
    [Theory]
    [InlineData("gpt-4o-mini", AICapabilityType.ChatCompletion, true)]
    [InlineData("gpt-4o-mini", AICapabilityType.Embeddings, false)]
    [InlineData("gpt-4o-mini", AICapabilityType.ImageGeneration, false)]
    [InlineData("gpt-4o-mini", AICapabilityType.AudioTranscription, false)]
    [InlineData("openai/text-embedding-3-small", AICapabilityType.Embeddings, true)]
    [InlineData("openai/text-embedding-3-small", AICapabilityType.ChatCompletion, false)]
    [InlineData("openrouter/nvidia/nemotron-3-embed-1b:free", AICapabilityType.Embeddings, true)]
    [InlineData("whisper-1", AICapabilityType.AudioTranscription, true)]
    [InlineData("whisper-1", AICapabilityType.ChatCompletion, false)]
    [InlineData("gpt-4o-mini-transcribe", AICapabilityType.AudioTranscription, true)]
    [InlineData("tts-1-hd", AICapabilityType.TextToSpeech, true)]
    [InlineData("tts-1-hd", AICapabilityType.ChatCompletion, false)]
    [InlineData("gpt-4o-mini-tts", AICapabilityType.TextToSpeech, true)]
    [InlineData("dall-e-3", AICapabilityType.ImageGeneration, true)]
    [InlineData("dall-e-3", AICapabilityType.ChatCompletion, false)]
    [InlineData("gpt-image-1", AICapabilityType.ImageGeneration, true)]
    [InlineData("gpt-4o", AICapabilityType.VisionAnalysis, true)]
    [InlineData("gpt-4o", AICapabilityType.ChatCompletion, true)]
    [InlineData("llama-3.1-8b-instruct", AICapabilityType.VisionAnalysis, true)]
    [InlineData("llama-3.1-8b-instruct", AICapabilityType.WebSearch, true)]
    public void Model_id_heuristics_match_capability(string modelId, AICapabilityType capability, bool expected)
    {
        ProviderModelCapabilityClassifier.Matches(modelId, capability).ShouldBe(expected);
    }

    [Fact]
    public void OpenRouter_embedding_metadata_wins_over_chat_looking_id()
    {
        var hints = new ProviderModelDiscoveryHints
        {
            OutputModalities = new[] { "embeddings" },
            InputModalities = new[] { "text" }
        };

        ProviderModelCapabilityClassifier.Matches("custom/router-model", AICapabilityType.Embeddings, hints)
            .ShouldBeTrue();
        ProviderModelCapabilityClassifier.Matches("custom/router-model", AICapabilityType.ChatCompletion, hints)
            .ShouldBeFalse();
    }

    [Fact]
    public void LiteLlm_mode_classifies_transcription()
    {
        var hints = new ProviderModelDiscoveryHints { Mode = "audio_transcription" };

        ProviderModelCapabilityClassifier.Matches("vendor/generic-1", AICapabilityType.AudioTranscription, hints)
            .ShouldBeTrue();
        ProviderModelCapabilityClassifier.Matches("vendor/generic-1", AICapabilityType.ChatCompletion, hints)
            .ShouldBeFalse();
    }

    [Fact]
    public void OpenRouter_image_output_is_image_generation()
    {
        var hints = new ProviderModelDiscoveryHints
        {
            InputModalities = new[] { "text" },
            OutputModalities = new[] { "image" }
        };

        ProviderModelCapabilityClassifier.Matches("black-forest-labs/flux", AICapabilityType.ImageGeneration, hints)
            .ShouldBeTrue();
        ProviderModelCapabilityClassifier.Matches("black-forest-labs/flux", AICapabilityType.ChatCompletion, hints)
            .ShouldBeFalse();
    }
}
