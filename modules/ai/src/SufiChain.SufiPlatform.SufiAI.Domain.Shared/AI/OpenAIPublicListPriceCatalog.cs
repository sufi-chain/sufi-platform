using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// Operator hint for OpenAI's published standard short-context USD rates per 1M tokens.
/// This is not returned by <c>GET /v1/models</c> and is not billed from the provider.
/// </summary>
public sealed class OpenAIPublicListPrice
{
    public OpenAIPublicListPrice(
        string modelId,
        decimal inputCostPer1MTokens,
        decimal? outputCostPer1MTokens)
    {
        ModelId = modelId;
        InputCostPer1MTokens = inputCostPer1MTokens;
        OutputCostPer1MTokens = outputCostPer1MTokens;
    }

    public string ModelId { get; }

    public decimal InputCostPer1MTokens { get; }

    public decimal? OutputCostPer1MTokens { get; }
}

public static class OpenAIPublicListPriceCatalog
{
    public const string SourceUrl = "https://developers.openai.com/api/docs/pricing";

    public const string AsOf = "2026-09-07";

    private static readonly Regex DatedSnapshot = new Regex(
        @"^(?<base>.+)-\d{4}-\d{2}-\d{2}$",
        RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly Dictionary<string, OpenAIPublicListPrice> Prices =
        CreatePrices();

    public static bool TryGet(string? modelId, out OpenAIPublicListPrice price)
    {
        price = null!;
        if (string.IsNullOrWhiteSpace(modelId))
        {
            return false;
        }

        var id = modelId.Trim();
        if (Prices.TryGetValue(id, out price))
        {
            return true;
        }

        var match = DatedSnapshot.Match(id);
        return match.Success && Prices.TryGetValue(match.Groups["base"].Value, out price);
    }

    private static Dictionary<string, OpenAIPublicListPrice> CreatePrices()
    {
        var prices = new Dictionary<string, OpenAIPublicListPrice>(StringComparer.OrdinalIgnoreCase);
        foreach (var row in StandardShortContextRows())
        {
            prices[row.ModelId] = row;
        }

        prices["gpt-4-turbo"] = prices["gpt-4-turbo-2024-04-09"];
        prices["gpt-daybreak-blue-latest"] = prices["gpt-5.6-sol"];
        prices["gpt-daybreak-red-latest"] = prices["gpt-5.6-cyber"];
        return prices;
    }

    private static IEnumerable<OpenAIPublicListPrice> StandardShortContextRows()
    {
        yield return Row("gpt-6-astra", 10.00m, 50.00m);
        yield return Row("gpt-5.6-sol", 4.00m, 20.00m);
        yield return Row("gpt-5.6-terra", 2.00m, 12.00m);
        yield return Row("gpt-5.6-luna", 0.20m, 1.20m);
        yield return Row("gpt-5.6-cyber", 12.50m, 75.00m);
        yield return Row("gpt-5.5", 5.00m, 30.00m);
        yield return Row("gpt-5.5-pro", 30.00m, 180.00m);
        yield return Row("gpt-5.5-cyber", 12.50m, 75.00m);
        yield return Row("gpt-5.4", 2.50m, 15.00m);
        yield return Row("gpt-5.4-mini", 0.75m, 4.50m);
        yield return Row("gpt-5.4-nano", 0.20m, 1.25m);
        yield return Row("gpt-5.4-pro", 30.00m, 180.00m);
        yield return Row("gpt-5.3-codex", 1.75m, 14.00m);
        yield return Row("gpt-5.2", 1.75m, 14.00m);
        yield return Row("gpt-5.2-pro", 21.00m, 168.00m);
        yield return Row("gpt-5.1", 1.25m, 10.00m);
        yield return Row("gpt-5", 1.25m, 10.00m);
        yield return Row("gpt-5-mini", 0.25m, 2.00m);
        yield return Row("gpt-5-nano", 0.05m, 0.40m);
        yield return Row("gpt-5-pro", 15.00m, 120.00m);
        yield return Row("gpt-5-search-api", 1.25m, 10.00m);
        yield return Row("gpt-4.1", 2.00m, 8.00m);
        yield return Row("gpt-4.1-mini", 0.40m, 1.60m);
        yield return Row("gpt-4.1-nano", 0.10m, 0.40m);
        yield return Row("gpt-4o", 2.50m, 10.00m);
        yield return Row("gpt-4o-2024-05-13", 5.00m, 15.00m);
        yield return Row("gpt-4o-mini", 0.15m, 0.60m);
        yield return Row("o1", 15.00m, 60.00m);
        yield return Row("o1-pro", 150.00m, 600.00m);
        yield return Row("o3-pro", 20.00m, 80.00m);
        yield return Row("o3", 2.00m, 8.00m);
        yield return Row("o4-mini", 1.10m, 4.40m);
        yield return Row("o3-mini", 1.10m, 4.40m);
        yield return Row("gpt-4-turbo-2024-04-09", 10.00m, 30.00m);
        yield return Row("gpt-4-0613", 30.00m, 60.00m);
        yield return Row("gpt-3.5-turbo", 0.50m, 1.50m);
        yield return Row("gpt-3.5-turbo-0125", 0.50m, 1.50m);
        yield return Row("gpt-3.5-turbo-1106", 1.00m, 2.00m);
        yield return Row("gpt-3.5-turbo-instruct", 1.50m, 2.00m);
        yield return Row("text-embedding-3-small", 0.02m, null);
        yield return Row("text-embedding-3-large", 0.13m, null);
        yield return Row("text-embedding-ada-002", 0.10m, null);
        yield return Row("gpt-4o-transcribe", 2.50m, 10.00m);
        yield return Row("gpt-4o-mini-transcribe", 1.25m, 5.00m);
        yield return Row("gpt-4o-transcribe-diarize", 2.50m, 10.00m);
    }

    private static OpenAIPublicListPrice Row(string modelId, decimal input, decimal? output)
    {
        return new OpenAIPublicListPrice(modelId, input, output);
    }
}
