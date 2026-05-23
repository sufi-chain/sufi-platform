using System.ComponentModel.DataAnnotations;

namespace SufiChain.SufiAbp.ShortLinkGenerator;

/// <summary>
/// Editable short-link generator settings exposed in the settings UI.
/// </summary>
public class ShortLinkGeneratorSettingsDto
{
    [StringLength(ShortLinkGeneratorConsts.ShortUrl.MaxDestinationUrlLength)]
    public string? BaseUrl { get; set; }

    [Range(1, ShortLinkGeneratorConsts.ShortUrl.MaxShortCodeLength)]
    public int ShortCodeLength { get; set; } = ShortLinkGeneratorConsts.DefaultShortCodeLength;

    [Range(1, int.MaxValue)]
    public int CacheExpirationMinutes { get; set; } = ShortLinkGeneratorConsts.DefaultCacheExpirationMinutes;

    [Range(1, int.MaxValue)]
    public int DefaultExpirationDays { get; set; } = ShortLinkGeneratorConsts.DefaultExpirationDays;
}
