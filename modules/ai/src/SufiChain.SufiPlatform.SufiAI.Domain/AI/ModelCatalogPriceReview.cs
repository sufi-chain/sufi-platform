using System.Collections.Generic;

namespace SufiChain.SufiPlatform.SufiAI;

public static class ModelCatalogPriceReview
{
    public static bool TryGetReplacement(
        AIModelConfiguration route,
        IReadOnlyList<ModelCatalogEntry>? catalog,
        decimal markupPercent,
        out CatalogPriceQuote quote)
    {
        quote = default;
        if (catalog == null)
        {
            return false;
        }

        var match = ModelCatalogMatcher.Match(catalog, route.ModelId);
        if (match == null)
        {
            return false;
        }

        quote = CatalogPriceQuote.From(
            route.CapabilityType,
            match.PromptPricePerToken,
            match.CompletionPricePerToken,
            markupPercent,
            match.ImagePrice,
            match.ImageOutputPrice,
            match.ImageTokenPrice,
            match.RequestPrice,
            match.WebSearchPrice);
        return quote.InputPrice != route.InputPrice ||
               quote.OutputPrice != route.OutputPrice ||
               quote.InputUnit != route.InputPriceUnit ||
               quote.OutputUnit != route.OutputPriceUnit;
    }
}
