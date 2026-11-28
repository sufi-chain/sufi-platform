namespace SufiChain.SufiPlatform.ShortLinks;

/// <summary>
/// Grid status for a short link. Stored <c>IsActive</c> is unchanged.
/// A past <c>ExpiresAt</c> is expired, matching redirect validity.
/// </summary>
public static class ShortLinkExpiry
{
    public static bool IsExpired(DateTime? expiresAt, DateTime utcNow) =>
        expiresAt.HasValue && expiresAt.Value < utcNow;
}
