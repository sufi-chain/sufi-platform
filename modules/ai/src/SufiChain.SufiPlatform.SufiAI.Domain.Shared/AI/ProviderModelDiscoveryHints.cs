using System.Collections.Generic;

namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// Optional fields from an OpenAI-compatible <c>/models</c> payload.
/// OpenRouter uses <c>architecture</c> modalities; LiteLLM often sends <c>mode</c>.
/// </summary>
public sealed class ProviderModelDiscoveryHints
{
    public string? Mode { get; set; }

    public string? Modality { get; set; }

    public IReadOnlyList<string>? InputModalities { get; set; }

    public IReadOnlyList<string>? OutputModalities { get; set; }
}
