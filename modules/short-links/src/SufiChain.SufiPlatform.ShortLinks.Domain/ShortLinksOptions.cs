namespace SufiChain.SufiPlatform.ShortLinks;

public class ShortLinksOptions
{
    /// <summary>
    /// Base URL for the application. Read from App:SelfUrl configuration.
    /// Example: "https://localhost:1404"
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// The required base key for public short URL redirects.
    /// Final public URLs are built as {BaseUrl}/{RedirectRoute}/{ShortCode}.
    /// Default is "short-links".
    /// Configure in appsettings.json: "ShortLinks:RedirectRoute"
    /// </summary>
    public string RedirectRoute { get; set; } = ShortLinksConsts.DefaultRedirectRoute;
    
    /// <summary>
    /// Length of generated short codes. Default is 8.
    /// </summary>
    public int ShortCodeLength { get; set; } = ShortLinksConsts.DefaultShortCodeLength;
    
    /// <summary>
    /// Cache expiration time in minutes for short URL lookups. Default is 60.
    /// </summary>
    public int CacheExpirationMinutes { get; set; } = ShortLinksConsts.DefaultCacheExpirationMinutes;
    
    /// <summary>
    /// Default expiration days for new short URLs. Null means no expiration. Default is 365.
    /// </summary>
    public int? DefaultExpirationDays { get; set; } = ShortLinksConsts.DefaultExpirationDays;
}

