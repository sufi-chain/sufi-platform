using System;
using System.Collections.Generic;

namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// Maps OpenAI-compatible catalog rows onto workspace capability routes.
/// Provider metadata wins; model-id heuristics cover OpenAI-style lists that
/// only return <c>id</c> / <c>owned_by</c>.
/// </summary>
public static class ProviderModelCapabilityClassifier
{
    public static bool Matches(
        string? modelId,
        AICapabilityType capabilityType,
        ProviderModelDiscoveryHints? hints = null)
    {
        var kinds = Classify(modelId, hints);
        return capabilityType switch
        {
            AICapabilityType.Embeddings => kinds.HasFlag(ProviderModelKinds.Embeddings),
            AICapabilityType.AudioTranscription => kinds.HasFlag(ProviderModelKinds.AudioTranscription),
            AICapabilityType.TextToSpeech => kinds.HasFlag(ProviderModelKinds.TextToSpeech),
            AICapabilityType.ImageGeneration => kinds.HasFlag(ProviderModelKinds.ImageGeneration),
            AICapabilityType.VisionAnalysis => kinds.HasFlag(ProviderModelKinds.Vision) ||
                                               kinds.HasFlag(ProviderModelKinds.Chat),
            AICapabilityType.ChatCompletion or
                AICapabilityType.WebSearch or
                AICapabilityType.WebFetch => kinds.HasFlag(ProviderModelKinds.Chat),
            _ => kinds.HasFlag(ProviderModelKinds.Chat)
        };
    }

    public static ProviderModelKinds Classify(string? modelId, ProviderModelDiscoveryHints? hints = null)
    {
        var fromHints = ClassifyFromHints(hints);
        if (fromHints != ProviderModelKinds.None)
        {
            return fromHints;
        }

        return ClassifyFromModelId(modelId);
    }

    private static ProviderModelKinds ClassifyFromHints(ProviderModelDiscoveryHints? hints)
    {
        if (hints == null)
        {
            return ProviderModelKinds.None;
        }

        var fromMode = ClassifyLiteLlmMode(hints.Mode);
        if (fromMode != ProviderModelKinds.None)
        {
            return fromMode;
        }

        var outputs = NormalizeTokens(hints.OutputModalities);
        var inputs = NormalizeTokens(hints.InputModalities);
        var modality = Normalize(hints.Modality);

        if (outputs.Count == 0 && inputs.Count == 0 && modality.Length == 0)
        {
            return ProviderModelKinds.None;
        }

        if (ContainsAny(outputs, "embedding", "embeddings") ||
            modality.Contains("embedding", StringComparison.Ordinal))
        {
            return ProviderModelKinds.Embeddings;
        }

        if (ContainsAny(outputs, "image", "images") && !ContainsAny(outputs, "text"))
        {
            return ProviderModelKinds.ImageGeneration;
        }

        if (ContainsAny(outputs, "audio", "speech") && !ContainsAny(inputs, "audio"))
        {
            return ProviderModelKinds.TextToSpeech;
        }

        if (ContainsAny(inputs, "audio") && ContainsAny(outputs, "text"))
        {
            return ProviderModelKinds.AudioTranscription;
        }

        var kinds = ProviderModelKinds.None;
        if (ContainsAny(outputs, "text") || modality.Contains("text->text", StringComparison.Ordinal))
        {
            kinds |= ProviderModelKinds.Chat;
        }

        if (ContainsAny(inputs, "image", "images") && ContainsAny(outputs, "text"))
        {
            kinds |= ProviderModelKinds.Vision | ProviderModelKinds.Chat;
        }

        if (ContainsAny(outputs, "image", "images") && ContainsAny(outputs, "text"))
        {
            kinds |= ProviderModelKinds.Chat | ProviderModelKinds.ImageGeneration;
        }

        return kinds;
    }

    private static ProviderModelKinds ClassifyLiteLlmMode(string? mode)
    {
        var normalized = Normalize(mode);
        if (normalized.Length == 0)
        {
            return ProviderModelKinds.None;
        }

        if (normalized is "embedding" or "embeddings")
        {
            return ProviderModelKinds.Embeddings;
        }

        if (normalized is "image_generation" or "image-generation" or "image")
        {
            return ProviderModelKinds.ImageGeneration;
        }

        if (normalized is "audio_transcription" or "audio-transcription" or "transcription")
        {
            return ProviderModelKinds.AudioTranscription;
        }

        if (normalized is "audio_speech" or "audio-speech" or "audio_generation" or "tts")
        {
            return ProviderModelKinds.TextToSpeech;
        }

        if (normalized is "chat" or "completion" or "responses")
        {
            return ProviderModelKinds.Chat;
        }

        return ProviderModelKinds.None;
    }

    private static ProviderModelKinds ClassifyFromModelId(string? modelId)
    {
        var id = Normalize(modelId);
        if (id.Length == 0)
        {
            return ProviderModelKinds.None;
        }

        if (IsEmbeddingModelId(modelId, id))
        {
            return ProviderModelKinds.Embeddings;
        }

        if (ContainsAny(id, "dall-e", "dall.e", "gpt-image", "imagen", "stable-diffusion",
                "stable-diff", "sdxl", "black-forest", "recraft", "ideogram", "seedream",
                "flux.", "flux-", "/flux", "playground-v", "kandinsky", "luma-photon"))
        {
            return ProviderModelKinds.ImageGeneration;
        }

        if (ContainsAny(id, "tts-", "-tts", "_tts", "/tts", "text-to-speech", "gpt-4o-mini-tts",
                "gpt-4o-tts"))
        {
            return ProviderModelKinds.TextToSpeech;
        }

        if (ContainsAny(id, "whisper", "transcri", "speech-to-text", "parakeet") ||
            (id.Contains("asr", StringComparison.Ordinal) &&
             !id.Contains("chat", StringComparison.Ordinal)))
        {
            return ProviderModelKinds.AudioTranscription;
        }

        var kinds = ProviderModelKinds.Chat;
        if (ContainsAny(id, "vision", "-vl", "_vl", "/vl", "pixtral", "llava", "internvl",
                "gpt-4o", "gpt-4.1", "gpt-4-turbo", "gpt-4-vision", "o3", "o4",
                "claude-3", "claude-4", "gemini", "qwen-vl", "qwen2-vl", "qwen2.5-vl"))
        {
            kinds |= ProviderModelKinds.Vision;
        }

        return kinds;
    }

    private static bool IsEmbeddingModelId(string? originalId, string normalizedId)
    {
        if (EmbeddingModelDefaults.IsKnownModel(originalId))
        {
            return true;
        }

        return normalizedId.Contains("embedding", StringComparison.Ordinal) ||
               ContainsAny(normalizedId, "-embed", "_embed", "/embed", "embed-", "embed_", "embed/");
    }

    private static HashSet<string> NormalizeTokens(IReadOnlyList<string>? values)
    {
        var set = new HashSet<string>(StringComparer.Ordinal);
        if (values == null)
        {
            return set;
        }

        foreach (var value in values)
        {
            var normalized = Normalize(value);
            if (normalized.Length > 0)
            {
                set.Add(normalized);
            }
        }

        return set;
    }

    private static bool ContainsAny(HashSet<string> values, params string[] needles)
    {
        foreach (var needle in needles)
        {
            if (values.Contains(needle))
            {
                return true;
            }
        }

        return false;
    }

    private static bool ContainsAny(string value, params string[] needles)
    {
        foreach (var needle in needles)
        {
            if (value.Contains(needle, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static string Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToLowerInvariant();
    }
}
