using System;

namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// Resolves a sensible default embedding dimension for a model when the configured
/// <see cref="AIModelConfiguration.Dimensions"/> is left blank. Dimensions are inferred from
/// well-known model identifiers (case-insensitive). Unknown models use the compatibility
/// fallback until the provider probe supplies the authoritative dimension.
/// Native sizes for OpenRouter embedding models were taken from
/// https://openrouter.ai/models?output_modalities=embeddings and
/// https://openrouter.ai/api/v1/models?output_modalities=embeddings on 2026-09-09
/// (37 models). Matryoshka models use the native/max size, not a reduced slice.
/// </summary>
public static class EmbeddingModelDefaults
{
    /// <summary>
    /// Safe fallback used when a model is unrecognized or the dimension cannot be inferred.
    /// </summary>
    public const int FallbackDimensions = 1536;

    /// <summary>
    /// Conservative token window used when the model is unrecognized.
    /// </summary>
    public const int FallbackMaxInputTokens = 4096;

    public const int OpenAiEmbeddingMaxInputTokens = 8191;

    public const int NvidiaEmbeddingMaxInputTokens = 4096;

    /// <summary>
    /// Longer needles first so <c>voyage-4-large</c> does not match <c>voyage-4</c>.
    /// </summary>
    private static readonly (string Needle, int Dimensions)[] KnownDimensions =
    {
        ("voyage-multimodal-3.5", 2048),
        ("voyage-code-4", 2048),
        ("voyage-4-large", 2048),
        ("voyage-4-lite", 2048),
        ("voyage-4", 2048),
        ("text-embedding-3-large", 3072),
        ("text-embedding-3-small", 1536),
        ("text-embedding-ada-002", 1536),
        ("qwen3-embedding-8b", 4096),
        ("qwen3-embedding-4b", 2560),
        ("gemini-embedding-2", 3072),
        ("gemini-embedding-001", 3072),
        ("pplx-embed-v1-4b", 2560),
        ("pplx-embed-v1-0.6b", 1024),
        ("nemotron-3-embed-1b", 2048),
        ("llama-nemotron-embed-vl-1b-v2", 2048),
        ("codestral-embed-2505", 1536),
        ("mistral-embed", 1024),
        ("lfm-2.5-embedding", 1024),
        ("lfm2.5-embedding", 1024),
        ("gte-large", 1024),
        ("gte-base", 768),
        ("multilingual-e5-large", 1024),
        ("e5-large-v2", 1024),
        ("e5-base-v2", 768),
        ("bge-large-en-v1.5", 1024),
        ("bge-base-en-v1.5", 768),
        ("bge-m3", 1024),
        ("paraphrase-minilm-l6-v2", 384),
        ("all-minilm-l12-v2", 384),
        ("all-minilm-l6-v2", 384),
        ("multi-qa-mpnet-base-dot-v1", 768),
        ("all-mpnet-base-v2", 768)
    };

    public static int GetDimensions(string? modelId)
    {
        if (string.IsNullOrWhiteSpace(modelId))
        {
            return FallbackDimensions;
        }

        var id = modelId.Trim();
        foreach (var (needle, dimensions) in KnownDimensions)
        {
            if (id.Contains(needle, StringComparison.OrdinalIgnoreCase))
            {
                return dimensions;
            }
        }

        return FallbackDimensions;
    }

    public static int GetMaxInputTokens(string? modelId)
    {
        if (IsNvidiaEmbeddingModel(modelId))
        {
            return NvidiaEmbeddingMaxInputTokens;
        }

        if (IsOpenAiEmbeddingModel(modelId))
        {
            return OpenAiEmbeddingMaxInputTokens;
        }

        return FallbackMaxInputTokens;
    }

    /// <summary>
    /// Nvidia embedding endpoints reject Semantic Kernel's default
    /// <c>encoding_format=base64</c>. Use float for those models regardless of
    /// gateway URL (OpenRouter, <c>ai.sufichain.com</c>, or a private proxy).
    /// </summary>
    public static string? GetEncodingFormat(string? modelId, string? apiBaseUrl)
    {
        _ = apiBaseUrl;
        return IsNvidiaEmbeddingModel(modelId) ? "float" : null;
    }

    private static bool IsNvidiaEmbeddingModel(string? modelId)
    {
        if (string.IsNullOrWhiteSpace(modelId))
        {
            return false;
        }

        var id = modelId.Trim();
        if (id.StartsWith("nvidia/", StringComparison.OrdinalIgnoreCase) ||
            id.Contains("/nvidia/", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return id.Contains("nemotron", StringComparison.OrdinalIgnoreCase) &&
               id.Contains("embed", StringComparison.OrdinalIgnoreCase);
    }

    public static bool SupportsInputTruncation(string? modelId) => IsNvidiaEmbeddingModel(modelId);

    private static bool IsOpenAiEmbeddingModel(string? modelId)
    {
        if (string.IsNullOrWhiteSpace(modelId))
        {
            return false;
        }

        var id = modelId.Trim();
        return id.Contains("text-embedding-3-small", StringComparison.OrdinalIgnoreCase) ||
               id.Contains("text-embedding-3-large", StringComparison.OrdinalIgnoreCase) ||
               id.Contains("text-embedding-ada-002", StringComparison.OrdinalIgnoreCase);
    }

    public static bool RequiresCustomTransport(string? modelId, string? apiBaseUrl)
    {
        return string.Equals(GetEncodingFormat(modelId, apiBaseUrl), "float", StringComparison.OrdinalIgnoreCase);
    }
}
