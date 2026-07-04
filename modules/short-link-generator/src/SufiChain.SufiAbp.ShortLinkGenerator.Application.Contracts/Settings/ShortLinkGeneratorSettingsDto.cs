using System.ComponentModel.DataAnnotations;

namespace SufiChain.SufiAbp.ShortLinkGenerator;

/// <summary>
/// Editable short-link generator settings exposed in the settings UI.
/// </summary>
public class ShortLinkGeneratorSettingsDto
{
    [StringLength(ShortLinkGeneratorConsts.ShortUrl.MaxDestinationUrlLength)]
    public string? BaseUrl { get; set; }

    [Required]
    [StringLength(ShortLinkGeneratorConsts.ShortUrl.MaxRedirectRouteLength)]
    [RegularExpression(@"^[A-Za-z0-9][A-Za-z0-9_-]*$")]
    public string RedirectRoute { get; set; } = ShortLinkGeneratorConsts.DefaultRedirectRoute;

    [Range(1, ShortLinkGeneratorConsts.ShortUrl.MaxShortCodeLength)]
    public int ShortCodeLength { get; set; } = ShortLinkGeneratorConsts.DefaultShortCodeLength;

    [Range(1, int.MaxValue)]
    public int CacheExpirationMinutes { get; set; } = ShortLinkGeneratorConsts.DefaultCacheExpirationMinutes;

    [Range(1, int.MaxValue)]
    public int DefaultExpirationDays { get; set; } = ShortLinkGeneratorConsts.DefaultExpirationDays;
}
