using SufiChain.SufiPlatform.SufiAI.Workspaces;

namespace SufiChain.SufiPlatform.SufiAI.Catalog;

public static class AvailableModelListComposer
{
    public static List<OpenAIModelDto> Compose(
        IReadOnlyList<OpenAIModelDto> workspaceModels,
        IReadOnlyList<ModelCatalogEntry>? catalog,
        AICapabilityType capabilityType,
        decimal markupPercent)
    {
        if (capabilityType == AICapabilityType.VisionAnalysis)
        {
            return new List<OpenAIModelDto>();
        }

        var enriched = workspaceModels.Select(model => Enrich(model, catalog, capabilityType, markupPercent)).ToList();
        if (IsNonText(capabilityType))
        {
            var fromCatalog = catalog == null
                ? new List<OpenAIModelDto>()
                : catalog
                    .Where(entry => Matches(entry, capabilityType))
                    .Select(entry => FromCatalog(entry, capabilityType, markupPercent))
                    .ToList();
            var fromWorkspace = enriched.Where(model => HasDeclaredMatch(model, capabilityType)).ToList();
            return fromCatalog
                .Concat(fromWorkspace)
                .GroupBy(model => model.Id, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .OrderBy(model => model.Id, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        if (catalog == null)
        {
            return enriched.OrderBy(model => model.Id, StringComparer.OrdinalIgnoreCase).ToList();
        }

        return enriched
            .Where(model => HasDeclaredMatch(model, capabilityType))
            .OrderBy(model => model.Id, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static bool IsNonText(AICapabilityType capabilityType)
    {
        return capabilityType is AICapabilityType.Embeddings
            or AICapabilityType.AudioTranscription
            or AICapabilityType.TextToSpeech
            or AICapabilityType.ImageGeneration
            or AICapabilityType.Decisions;
    }

    private static OpenAIModelDto Enrich(OpenAIModelDto model, IReadOnlyList<ModelCatalogEntry>? catalog, AICapabilityType capabilityType, decimal markupPercent)
    {
        var match = ModelCatalogMatcher.Match(catalog, model.Id);
        if (match == null)
        {
            ApplyPrice(model, capabilityType, markupPercent);
            ApplyFlags(model);
            return model;
        }

        model.InputModalities ??= Copy(match.InputModalities);
        model.OutputModalities ??= Copy(match.OutputModalities);
        model.SupportedParameters ??= Copy(match.SupportedParameters);
        model.SupportedEndpoints ??= Copy(match.SupportedEndpoints);
        model.Mode ??= match.Mode;
        if (model.ContextLength is null or <= 0 && match.ContextLength is > 0)
        {
            model.ContextLength = match.ContextLength;
        }

        if (model.ReasoningEfforts is not { Count: > 0 } && match.ReasoningEfforts.Count > 0)
        {
            model.ReasoningEfforts = match.ReasoningEfforts.ToList();
        }

        if (string.IsNullOrWhiteSpace(model.DefaultReasoningEffort))
        {
            model.DefaultReasoningEffort = match.DefaultReasoningEffort;
        }

        if (string.IsNullOrWhiteSpace(model.DisplayName))
        {
            model.DisplayName = match.Name;
        }

        if (string.IsNullOrWhiteSpace(model.IconUrl))
        {
            model.IconUrl = match.IconUrl;
        }

        model.PromptPricePerToken ??= match.PromptPricePerToken;
        model.CompletionPricePerToken ??= match.CompletionPricePerToken;
        model.ImagePrice ??= match.ImagePrice;
        model.ImageOutputPrice ??= match.ImageOutputPrice;
        model.ImageTokenPrice ??= match.ImageTokenPrice;
        model.RequestPrice ??= match.RequestPrice;
        model.WebSearchPrice ??= match.WebSearchPrice;
        ApplyPrice(model, capabilityType, markupPercent);
        ApplyFlags(model);
        return model;
    }

    private static OpenAIModelDto FromCatalog(ModelCatalogEntry entry, AICapabilityType capabilityType, decimal markupPercent)
    {
        var model = new OpenAIModelDto
        {
            Id = entry.Id,
            DisplayName = entry.Name,
            IconUrl = entry.IconUrl,
            Mode = entry.Mode,
            InputModalities = Copy(entry.InputModalities),
            OutputModalities = Copy(entry.OutputModalities),
            SupportedParameters = Copy(entry.SupportedParameters),
            SupportedEndpoints = Copy(entry.SupportedEndpoints),
            ContextLength = entry.ContextLength,
            ReasoningEfforts = Copy(entry.ReasoningEfforts),
            DefaultReasoningEffort = entry.DefaultReasoningEffort,
            PromptPricePerToken = entry.PromptPricePerToken,
            CompletionPricePerToken = entry.CompletionPricePerToken,
            ImagePrice = entry.ImagePrice,
            ImageOutputPrice = entry.ImageOutputPrice,
            ImageTokenPrice = entry.ImageTokenPrice,
            RequestPrice = entry.RequestPrice,
            WebSearchPrice = entry.WebSearchPrice
        };
        ApplyPrice(model, capabilityType, markupPercent);
        ApplyFlags(model);
        return model;
    }

    private static void ApplyPrice(OpenAIModelDto model, AICapabilityType capabilityType, decimal markupPercent)
    {
        var quote = CatalogPriceQuote.From(
            capabilityType,
            model.PromptPricePerToken,
            model.CompletionPricePerToken,
            markupPercent,
            model.ImagePrice,
            model.ImageOutputPrice,
            model.ImageTokenPrice,
            model.RequestPrice,
            model.WebSearchPrice);
        model.SuggestedInputPrice ??= quote.InputPrice;
        model.SuggestedInputPriceUnit = quote.InputUnit;
        model.SuggestedOutputPrice ??= quote.OutputPrice;
        model.SuggestedOutputPriceUnit = quote.OutputUnit;
        model.SuggestOutputPrice = quote.ShowOutput;
    }

    private static void ApplyFlags(OpenAIModelDto model)
    {
        var hints = Hints(model);
        model.AcceptsImageInput = ProviderModelCapabilityClassifier.AcceptsImageInput(hints);
        model.AcceptsFileInput = ProviderModelCapabilityClassifier.AcceptsFileInput(hints);
        model.SupportsReasoning = ProviderModelCapabilityClassifier.SupportsReasoning(hints) ||
                                  model.ReasoningEfforts is { Count: > 0 };
    }

    private static bool HasDeclaredMatch(OpenAIModelDto model, AICapabilityType capabilityType)
    {
        var hints = Hints(model);
        var hasMetadata = !string.IsNullOrWhiteSpace(hints.Mode) ||
                          hints.OutputModalities is { Count: > 0 } ||
                          !string.IsNullOrWhiteSpace(hints.Modality);
        return hasMetadata && ProviderModelCapabilityClassifier.Matches(model.Id, capabilityType, hints);
    }

    private static bool Matches(ModelCatalogEntry entry, AICapabilityType capabilityType)
    {
        return ProviderModelCapabilityClassifier.Matches(entry.Id, capabilityType, new ProviderModelDiscoveryHints
        {
            Mode = entry.Mode,
            InputModalities = entry.InputModalities,
            OutputModalities = entry.OutputModalities,
            SupportedParameters = entry.SupportedParameters
        });
    }

    private static ProviderModelDiscoveryHints Hints(OpenAIModelDto model)
    {
        return new ProviderModelDiscoveryHints
        {
            Mode = model.Mode,
            Modality = model.Modality,
            InputModalities = model.InputModalities,
            OutputModalities = model.OutputModalities,
            SupportedParameters = model.SupportedParameters
        };
    }

    private static List<string>? Copy(List<string>? values)
    {
        return values == null || values.Count == 0 ? null : values.ToList();
    }
}
