using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Domain.Tests.AI;

public class ProviderModelCapabilityClassifierTests
{
    [Theory]
    [InlineData("gpt-4o", AICapabilityType.VisionAnalysis)]
    [InlineData("gpt-4o", AICapabilityType.ChatCompletion)]
    [InlineData("llama-3.1-8b-instruct", AICapabilityType.VisionAnalysis)]
    [InlineData("whisper-1", AICapabilityType.AudioTranscription)]
    [InlineData("dall-e-3", AICapabilityType.ImageGeneration)]
    [InlineData("openai/text-embedding-3-small", AICapabilityType.Embeddings)]
    public void Model_id_alone_matches_nothing(string modelId, AICapabilityType capability)
    {
        ProviderModelCapabilityClassifier.Matches(modelId, capability).ShouldBeFalse();
    }

    [Fact]
    public void Output_modalities_select_route_lists()
    {
        ProviderModelCapabilityClassifier.Matches("m", AICapabilityType.ChatCompletion, Output("text")).ShouldBeTrue();
        ProviderModelCapabilityClassifier.Matches("m", AICapabilityType.ImageGeneration, Output("image")).ShouldBeTrue();
        ProviderModelCapabilityClassifier.Matches("m", AICapabilityType.ChatCompletion, Output("image")).ShouldBeFalse();
        ProviderModelCapabilityClassifier.Matches("m", AICapabilityType.TextToSpeech, Output("speech")).ShouldBeTrue();
        ProviderModelCapabilityClassifier.Matches("m", AICapabilityType.TextToSpeech, Output("audio")).ShouldBeFalse();
        ProviderModelCapabilityClassifier.Matches("m", AICapabilityType.AudioTranscription, Output("transcription")).ShouldBeTrue();
        ProviderModelCapabilityClassifier.Matches("m", AICapabilityType.Embeddings, Output("embeddings")).ShouldBeTrue();
        ProviderModelCapabilityClassifier.Matches("m", AICapabilityType.ChatCompletion, Output("video")).ShouldBeFalse();
        ProviderModelCapabilityClassifier.Matches("m", AICapabilityType.VisionAnalysis, new ProviderModelDiscoveryHints
        {
            InputModalities = new[] { "image", "text" },
            OutputModalities = new[] { "text" }
        }).ShouldBeFalse();
    }

    [Fact]
    public void Image_and_file_input_are_chat_flags()
    {
        var hints = new ProviderModelDiscoveryHints
        {
            InputModalities = new[] { "file", "image", "text" },
            OutputModalities = new[] { "text" },
            SupportedParameters = new[] { "reasoning" }
        };

        ProviderModelCapabilityClassifier.AcceptsImageInput(hints).ShouldBeTrue();
        ProviderModelCapabilityClassifier.AcceptsFileInput(hints).ShouldBeTrue();
        ProviderModelCapabilityClassifier.SupportsReasoning(hints).ShouldBeTrue();
    }

    private static ProviderModelDiscoveryHints Output(string modality)
    {
        return new ProviderModelDiscoveryHints { OutputModalities = new[] { modality } };
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
