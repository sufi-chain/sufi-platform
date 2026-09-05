using System;

namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// Resolves a sensible default embedding dimension for a model when the configured
/// <see cref="AIModelConfiguration.Dimensions"/> is left blank. Dimensions are inferred from
/// well-known model identifiers (case-insensitive). Unknown models use the compatibility
/// fallback until the provider probe supplies the authoritative dimension.
/// </summary>
public static class EmbeddingModelDefaults
{
    /// <summary>
    /// Safe fallback used when a model is unrecognized or the dimension cannot be inferred.
    /// </summary>
    public const int FallbackDimensions = 1536;

    public static int GetDimensions(string? modelId)
    {
        if (string.IsNullOrWhiteSpace(modelId))
        {
            return FallbackDimensions;
        }

        var id = modelId.Trim();

        if (id.Contains("text-embedding-3-large", StringComparison.OrdinalIgnoreCase))
        {
            return 3072;
        }

        if (id.Contains("text-embedding-3-small", StringComparison.OrdinalIgnoreCase) ||
            id.Contains("text-embedding-ada-002", StringComparison.OrdinalIgnoreCase))
        {
            return 1536;
        }

        if (id.Contains("nemotron-3-embed-1b", StringComparison.OrdinalIgnoreCase))
        {
            return 2048;
        }

        return FallbackDimensions;
    }

    public static string? GetEncodingFormat(string? modelId, string? apiBaseUrl)
    {
        if ((modelId?.Contains("nemotron", StringComparison.OrdinalIgnoreCase) ?? false) &&
            (apiBaseUrl?.Contains("openrouter.ai", StringComparison.OrdinalIgnoreCase) ?? false))
        {
            return "float";
        }

        return null;
    }

    public static bool RequiresCustomTransport(string? modelId, string? apiBaseUrl)
    {
        return string.Equals(GetEncodingFormat(modelId, apiBaseUrl), "float", StringComparison.OrdinalIgnoreCase);
    }
}
