using System;
using System.Collections.Generic;

namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// Maps catalog modalities onto workspace capability routes.
/// A row with no modalities and no LiteLLM mode matches nothing.
/// Model ids are not used. Image and file input do not create a vision route.
/// </summary>
public static class ProviderModelCapabilityClassifier
{
    public static bool Matches(
        string? modelId,
        AICapabilityType capabilityType,
        ProviderModelDiscoveryHints? hints = null)
    {
        _ = modelId;
        if (capabilityType == AICapabilityType.VisionAnalysis)
        {
            return false;
        }

        var kinds = Classify(modelId, hints);
        return capabilityType switch
        {
            AICapabilityType.Embeddings => kinds.HasFlag(ProviderModelKinds.Embeddings),
            AICapabilityType.AudioTranscription => kinds.HasFlag(ProviderModelKinds.AudioTranscription),
            AICapabilityType.TextToSpeech => kinds.HasFlag(ProviderModelKinds.TextToSpeech),
            AICapabilityType.ImageGeneration => kinds.HasFlag(ProviderModelKinds.ImageGeneration),
            AICapabilityType.ChatCompletion or
                AICapabilityType.WebSearch or
                AICapabilityType.WebFetch => kinds.HasFlag(ProviderModelKinds.Chat),
            AICapabilityType.Decisions => kinds.HasFlag(ProviderModelKinds.Decisions),
            _ => false
        };
    }

    public static ProviderModelKinds Classify(string? modelId, ProviderModelDiscoveryHints? hints = null)
    {
        _ = modelId;
        return ClassifyFromHints(hints);
    }

    public static bool AcceptsImageInput(ProviderModelDiscoveryHints? hints)
    {
        return ContainsToken(hints?.InputModalities, "image", "images");
    }

    public static bool AcceptsFileInput(ProviderModelDiscoveryHints? hints)
    {
        return ContainsToken(hints?.InputModalities, "file", "files");
    }

    public static bool SupportsReasoning(ProviderModelDiscoveryHints? hints)
    {
        return ContainsToken(
            hints?.SupportedParameters,
            "reasoning",
            "include_reasoning",
            "reasoning_effort");
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

        _ = inputs;
        if (ContainsAny(outputs, "embedding", "embeddings") ||
            modality.Contains("embedding", StringComparison.Ordinal))
        {
            return ProviderModelKinds.Embeddings;
        }

        var kinds = ProviderModelKinds.None;
        if (ContainsAny(outputs, "text") || modality.Contains("text->text", StringComparison.Ordinal))
        {
            kinds |= ProviderModelKinds.Chat;
        }

        if (ContainsAny(outputs, "image", "images"))
        {
            kinds |= ProviderModelKinds.ImageGeneration;
        }

        if (ContainsAny(outputs, "speech"))
        {
            kinds |= ProviderModelKinds.TextToSpeech;
        }

        if (ContainsAny(outputs, "transcription"))
        {
            kinds |= ProviderModelKinds.AudioTranscription;
        }

        if (ContainsAny(outputs, "decisions") || modality.Contains("decisions", StringComparison.Ordinal))
        {
            kinds |= ProviderModelKinds.Decisions;
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

        if (normalized is "decisions" or "decision")
        {
            return ProviderModelKinds.Decisions;
        }

        return ProviderModelKinds.None;
    }

    private static bool ContainsToken(IReadOnlyList<string>? values, params string[] needles)
    {
        return ContainsAny(NormalizeTokens(values), needles);
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
