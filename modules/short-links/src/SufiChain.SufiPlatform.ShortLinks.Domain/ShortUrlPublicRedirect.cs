namespace SufiChain.SufiPlatform.ShortLinks;

/// <summary>
/// Result of resolving <c>/s/{code}</c> for the current tenant.
/// A null result is not a short link. A known code with no destination is expired or inactive.
/// </summary>
public sealed class ShortUrlPublicRedirect
{
    public bool KnownCode { get; init; }

    public string? Destination { get; init; }
}
