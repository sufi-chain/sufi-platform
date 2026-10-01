using System.Collections.Generic;

namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// Normalized catalog row. Catalog providers map their own JSON into this shape.
/// <c>PromptPricePerToken</c> is the raw <c>pricing.prompt</c> value.
/// Its unit depends on the capability. <see cref="CatalogPriceQuote"/> converts it.
/// </summary>
public sealed class ModelCatalogEntry
{
    public string Id { get; set; } = string.Empty;

    /// <summary>Human-readable name when the catalog sends one.</summary>
    public string? Name { get; set; }

    /// <summary>Https icon URL when the catalog sends one. Other schemes are dropped.</summary>
    public string? IconUrl { get; set; }

    /// <summary>Provider mode such as chat, embedding, or decisions, when the catalog sends one.</summary>
    public string? Mode { get; set; }

    /// <summary>Relative paths the catalog says this model accepts, such as /v1/chat/completions.</summary>
    public List<string> SupportedEndpoints { get; set; } = new();

    public List<string> InputModalities { get; set; } = new();

    public List<string> OutputModalities { get; set; } = new();

    public List<string> SupportedParameters { get; set; } = new();

    /// <summary>OpenRouter <c>context_length</c>, in tokens.</summary>
    public int? ContextLength { get; set; }

    /// <summary>OpenRouter <c>reasoning.supported_efforts</c>, highest effort first.</summary>
    public List<string> ReasoningEfforts { get; set; } = new();

    /// <summary>OpenRouter <c>reasoning.default_effort</c>.</summary>
    public string? DefaultReasoningEffort { get; set; }

    /// <summary>Raw <c>pricing.prompt</c>. Not always USD per token.</summary>
    public decimal? PromptPricePerToken { get; set; }

    /// <summary>Raw <c>pricing.completion</c>. Not always USD per token.</summary>
    public decimal? CompletionPricePerToken { get; set; }

    /// <summary>USD per input image when the catalog sends <c>pricing.image</c>.</summary>
    public decimal? ImagePrice { get; set; }

    /// <summary>Raw <c>pricing.image_output</c>. Per image or per image token.</summary>
    public decimal? ImageOutputPrice { get; set; }

    /// <summary>USD per image token when the catalog sends <c>pricing.image_token</c>.</summary>
    public decimal? ImageTokenPrice { get; set; }

    /// <summary>USD per request when the catalog sends <c>pricing.request</c>.</summary>
    public decimal? RequestPrice { get; set; }

    /// <summary>USD per web search when the catalog sends <c>pricing.web_search</c>.</summary>
    public decimal? WebSearchPrice { get; set; }
}
