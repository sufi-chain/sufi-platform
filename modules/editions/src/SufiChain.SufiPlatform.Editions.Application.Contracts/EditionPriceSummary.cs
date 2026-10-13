namespace SufiChain.SufiPlatform.Editions;

/// <summary>
/// Optional plan price attached to an edition by a host module (for example SufiSaas).
/// Editions UI must not reference those modules directly.
/// </summary>
public class EditionPriceSummary
{
    public bool IsPriced { get; set; }

    public decimal? Amount { get; set; }

    public string? CurrencyCode { get; set; }
}
