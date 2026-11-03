using System.Collections.Generic;

namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// USD amount already expressed in <see cref="InputUnit"/> or <see cref="OutputUnit"/>.
/// A null or non-positive side is not a price.
/// </summary>
public readonly record struct SharedWorkspaceRoutePriceFloor(
    decimal? InputPrice,
    AIPriceUnit InputUnit,
    decimal? OutputPrice,
    AIPriceUnit OutputUnit);

/// <summary>
/// Parsed host floors. The numbers come from <see cref="SharedWorkspaceRoutePriceSettings"/>.
/// </summary>
public sealed class SharedWorkspaceRoutePriceSchedule
{
    public decimal? ChatInputPerMillionTokens { get; init; }
    public decimal? ChatOutputPerMillionTokens { get; init; }
    public decimal? EmbeddingsInputPerMillionTokens { get; init; }
    public decimal? AudioPerMinute { get; init; }
    public decimal? SpeechPerMillionCharacters { get; init; }
    public decimal? VisionInputPerMillionTokens { get; init; }
    public decimal? VisionOutputPerMillionTokens { get; init; }
    public decimal? ImagePerImage { get; init; }
    public decimal? WebPerRequest { get; init; }
    public decimal? DecisionsInputPerMillionTokens { get; init; }
    public decimal? DecisionsOutputPerMillionTokens { get; init; }

    public SharedWorkspaceRoutePriceFloor For(AICapabilityType capability)
    {
        return capability switch
        {
            AICapabilityType.ChatCompletion => TokenFloor(ChatInputPerMillionTokens, ChatOutputPerMillionTokens),
            AICapabilityType.VisionAnalysis => TokenFloor(VisionInputPerMillionTokens, VisionOutputPerMillionTokens),
            AICapabilityType.Embeddings => new SharedWorkspaceRoutePriceFloor(
                EmbeddingsInputPerMillionTokens,
                AIPriceUnit.PerMillionTokens,
                null,
                AIPriceUnit.PerMillionTokens),
            AICapabilityType.AudioTranscription => new SharedWorkspaceRoutePriceFloor(
                AudioPerMinute,
                AIPriceUnit.PerMinute,
                null,
                AIPriceUnit.PerMillionTokens),
            AICapabilityType.TextToSpeech => new SharedWorkspaceRoutePriceFloor(
                SpeechPerMillionCharacters,
                AIPriceUnit.PerMillionCharacters,
                null,
                AIPriceUnit.PerMillionTokens),
            AICapabilityType.ImageGeneration => new SharedWorkspaceRoutePriceFloor(
                null,
                AIPriceUnit.PerMillionTokens,
                ImagePerImage,
                AIPriceUnit.PerImage),
            AICapabilityType.WebSearch or AICapabilityType.WebFetch => new SharedWorkspaceRoutePriceFloor(
                WebPerRequest,
                AIPriceUnit.PerRequest,
                null,
                AIPriceUnit.PerRequest),
            AICapabilityType.Decisions => TokenFloor(DecisionsInputPerMillionTokens, DecisionsOutputPerMillionTokens),
            _ => new SharedWorkspaceRoutePriceFloor(null, AIPriceUnit.PerMillionTokens, null, AIPriceUnit.PerMillionTokens)
        };
    }

    private static SharedWorkspaceRoutePriceFloor TokenFloor(decimal? input, decimal? output)
    {
        return new SharedWorkspaceRoutePriceFloor(
            input,
            AIPriceUnit.PerMillionTokens,
            output,
            AIPriceUnit.PerMillionTokens);
    }
}

/// <summary>
/// Chooses the price written onto a shared-workspace route.
/// A positive catalog quote wins, then a positive price already on the route, then the host floor.
/// Zero is not a price. Passing no floor keeps the catalog-only review used for other workspaces.
/// </summary>
public static class SharedWorkspaceRoutePriceReview
{
    public static bool TryGetReplacement(
        AIModelConfiguration route,
        IReadOnlyList<ModelCatalogEntry>? catalog,
        decimal markupPercent,
        SharedWorkspaceRoutePriceFloor? sharedFloor,
        out CatalogPriceQuote quote)
    {
        if (sharedFloor == null)
        {
            return ModelCatalogPriceReview.TryGetReplacement(route, catalog, markupPercent, out quote);
        }

        quote = Resolve(route, catalog, markupPercent, sharedFloor.Value);
        return quote.InputPrice != route.InputPrice ||
               quote.OutputPrice != route.OutputPrice ||
               quote.InputUnit != route.InputPriceUnit ||
               quote.OutputUnit != route.OutputPriceUnit;
    }

    public static CatalogPriceQuote Resolve(
        AIModelConfiguration route,
        IReadOnlyList<ModelCatalogEntry>? catalog,
        decimal markupPercent,
        SharedWorkspaceRoutePriceFloor floor)
    {
        var catalogQuote = TryCatalogQuote(route, catalog, markupPercent);
        var basis = catalogQuote ?? CatalogPriceQuote.Default(route.CapabilityType);
        var input = FirstPositive(catalogQuote?.InputPrice, route.InputPrice, floor.InputPrice);
        var output = FirstPositive(catalogQuote?.OutputPrice, route.OutputPrice, floor.OutputPrice);
        return new CatalogPriceQuote(
            input,
            ChooseUnit(
                catalogQuote?.InputPrice,
                catalogQuote?.InputUnit,
                route.InputPrice,
                route.InputPriceUnit,
                floor.InputPrice,
                floor.InputUnit,
                basis.InputUnit),
            output,
            ChooseUnit(
                catalogQuote?.OutputPrice,
                catalogQuote?.OutputUnit,
                route.OutputPrice,
                route.OutputPriceUnit,
                floor.OutputPrice,
                floor.OutputUnit,
                basis.OutputUnit),
            basis.ShowOutput || output is > 0);
    }

    private static CatalogPriceQuote? TryCatalogQuote(
        AIModelConfiguration route,
        IReadOnlyList<ModelCatalogEntry>? catalog,
        decimal markupPercent)
    {
        if (catalog == null)
        {
            return null;
        }

        var match = ModelCatalogMatcher.Match(catalog, route.ModelId);
        if (match == null)
        {
            return null;
        }

        return CatalogPriceQuote.From(
            route.CapabilityType,
            match.PromptPricePerToken,
            match.CompletionPricePerToken,
            markupPercent,
            match.ImagePrice,
            match.ImageOutputPrice,
            match.ImageTokenPrice,
            match.RequestPrice,
            match.WebSearchPrice);
    }

    private static decimal? FirstPositive(params decimal?[] values)
    {
        foreach (var value in values)
        {
            if (value is > 0)
            {
                return value;
            }
        }

        return null;
    }

    private static AIPriceUnit ChooseUnit(
        decimal? catalogPrice,
        AIPriceUnit? catalogUnit,
        decimal? routePrice,
        AIPriceUnit routeUnit,
        decimal? floorPrice,
        AIPriceUnit floorUnit,
        AIPriceUnit fallback)
    {
        if (catalogPrice is > 0 && catalogUnit.HasValue)
        {
            return catalogUnit.Value;
        }

        if (routePrice is > 0)
        {
            return routeUnit;
        }

        if (floorPrice is > 0)
        {
            return floorUnit;
        }

        return fallback;
    }
}
