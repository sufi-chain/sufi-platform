using System.Collections.Generic;
using SufiChain.SufiPlatform.SufiAI;

namespace SufiChain.SufiPlatform.SufiAI.Workspaces;

public class OpenAIModelDto
{
    public string Id { get; set; } = string.Empty;

    /// <summary>Catalog display name when the provider sent one.</summary>
    public string? DisplayName { get; set; }

    /// <summary>Https icon URL from the catalog, when the provider sent one.</summary>
    public string? IconUrl { get; set; }

    public string? OwnedBy { get; set; }

    public long? Created { get; set; }

    /// <summary>LiteLLM-style catalog mode when the provider sends it.</summary>
    public string? Mode { get; set; }

    /// <summary>OpenRouter <c>architecture.modality</c> when present.</summary>
    public string? Modality { get; set; }

    public List<string>? InputModalities { get; set; }

    public List<string>? OutputModalities { get; set; }

    public List<string>? SupportedParameters { get; set; }

    public List<string>? SupportedEndpoints { get; set; }

    /// <summary>Provider context window in tokens, when the catalog sends one.</summary>
    public int? ContextLength { get; set; }

    /// <summary>Provider reasoning efforts, highest first.</summary>
    public List<string>? ReasoningEfforts { get; set; }

    /// <summary>Provider default reasoning effort.</summary>
    public string? DefaultReasoningEffort { get; set; }

    /// <summary>Raw catalog <c>pricing.prompt</c>. <see cref="CatalogPriceQuote"/> chooses the unit.</summary>
    public decimal? PromptPricePerToken { get; set; }

    /// <summary>Raw catalog <c>pricing.completion</c>. <see cref="CatalogPriceQuote"/> chooses the unit.</summary>
    public decimal? CompletionPricePerToken { get; set; }

    public decimal? ImagePrice { get; set; }

    public decimal? ImageOutputPrice { get; set; }

    public decimal? ImageTokenPrice { get; set; }

    public decimal? RequestPrice { get; set; }

    public decimal? WebSearchPrice { get; set; }

    public bool? AcceptsImageInput { get; set; }

    public bool? AcceptsFileInput { get; set; }

    public bool? SupportsReasoning { get; set; }

    public decimal? SuggestedInputPrice { get; set; }

    public AIPriceUnit SuggestedInputPriceUnit { get; set; }

    public decimal? SuggestedOutputPrice { get; set; }

    public AIPriceUnit SuggestedOutputPriceUnit { get; set; }

    public bool SuggestOutputPrice { get; set; }

    /// <summary>Token suggestion for the workspace fallback, which stays per million tokens.</summary>
    public decimal? SuggestedInputCostPer1MTokens =>
        SuggestedInputPriceUnit == AIPriceUnit.PerMillionTokens ? SuggestedInputPrice : null;

    /// <summary>Token suggestion for the workspace fallback, which stays per million tokens.</summary>
    public decimal? SuggestedOutputCostPer1MTokens =>
        SuggestedOutputPriceUnit == AIPriceUnit.PerMillionTokens ? SuggestedOutputPrice : null;
}
