using System.ComponentModel.DataAnnotations;

namespace SufiChain.SufiPlatform.ShortLinks;

/// <summary>
/// Editable short-link generator settings exposed in the settings UI.
/// </summary>
public class ShortLinksSettingsDto
{
    [StringLength(ShortLinksConsts.ShortUrl.MaxDestinationUrlLength)]
    public string? BaseUrl { get; set; }

    [Required]
    [StringLength(ShortLinksConsts.ShortUrl.MaxRedirectRouteLength)]
    [RegularExpression(@"^[A-Za-z0-9][A-Za-z0-9_-]*$")]
    public string RedirectRoute { get; set; } = ShortLinksConsts.DefaultRedirectRoute;

    [Range(1, ShortLinksConsts.ShortUrl.MaxShortCodeLength)]
    public int ShortCodeLength { get; set; } = ShortLinksConsts.DefaultShortCodeLength;

    [Range(1, int.MaxValue)]
    public int CacheExpirationMinutes { get; set; } = ShortLinksConsts.DefaultCacheExpirationMinutes;

    [Range(1, int.MaxValue)]
    public int DefaultExpirationDays { get; set; } = ShortLinksConsts.DefaultExpirationDays;
}
