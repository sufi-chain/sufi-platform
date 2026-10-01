using System.ComponentModel.DataAnnotations;

namespace SufiChain.SufiPlatform.Caching;

public class CacheManagementCacheDto
{
    public string CacheName { get; set; } = string.Empty;

    public string ItemType { get; set; } = string.Empty;

    public List<CacheRevocationDto> Revocations { get; set; } = new();
}

public class CacheRevocationDto
{
    public string? Key { get; set; }

    public DateTime RevokedUntilUtc { get; set; }
}

public class RevokeCacheInput
{
    public const int MaxDurationMinutes = 12 * 60;

    [Required]
    [StringLength(256)]
    public string CacheName { get; set; } = string.Empty;

    /// <summary>
    /// Empty revokes every key in the cache. A value revokes that key only.
    /// </summary>
    [StringLength(1024)]
    public string? Key { get; set; }

    [Range(1, MaxDurationMinutes)]
    public int DurationMinutes { get; set; } = 15;
}

public class LiftCacheRevocationInput
{
    [Required]
    [StringLength(256)]
    public string CacheName { get; set; } = string.Empty;

    /// <summary>
    /// Empty lifts the cache-wide revocation. A value lifts that key only.
    /// </summary>
    [StringLength(1024)]
    public string? Key { get; set; }
}
