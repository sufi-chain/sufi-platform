using System;

namespace SufiChain.SufiPlatform.SufiAI;

public static class ModelCatalogPriceCalculator
{
    public static decimal? MarkUpPerMillion(decimal? perTokenUsd, decimal percent)
    {
        if (perTokenUsd is not decimal rate || rate < 0)
        {
            return null;
        }

        return MarkUpAmount(rate * 1_000_000m, percent);
    }

    public static decimal? MarkUpAmount(decimal? amountUsd, decimal percent)
    {
        if (amountUsd is not decimal rate || rate < 0)
        {
            return null;
        }

        if (percent < 0)
        {
            percent = 0;
        }

        return decimal.Round(
            rate * (1m + percent / 100m),
            8,
            MidpointRounding.AwayFromZero);
    }
}
