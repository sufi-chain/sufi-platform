namespace SufiChain.SufiPlatform.Caching;

/// <summary>
/// One temporary revocation. A null or empty <see cref="Key"/> hides every entry in <see cref="CacheName"/>.
/// </summary>
public class CacheRevocationEntry
{
    public string CacheName { get; set; } = string.Empty;

    public string? Key { get; set; }

    public DateTime RevokedUntilUtc { get; set; }

    public bool AppliesTo(string cacheName, string? key)
    {
        if (!string.Equals(CacheName, cacheName, StringComparison.Ordinal))
        {
            return false;
        }

        return string.IsNullOrEmpty(Key) || string.Equals(Key, key, StringComparison.Ordinal);
    }
}
