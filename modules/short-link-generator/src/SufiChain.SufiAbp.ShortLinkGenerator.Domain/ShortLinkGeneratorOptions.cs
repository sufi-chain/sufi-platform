namespace SufiChain.SufiAbp.ShortLinkGenerator;

public class ShortLinkGeneratorOptions
{
    /// <summary>
    /// Base URL for the application. Read from App:SelfUrl configuration.
    /// Example: "https://localhost:1404"
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;
    
    /// <summary>
    /// The route prefix for short URL redirects. Default is "sur".
    /// Configure in appsettings.json: "ShortLinkGenerator:RedirectRoute"
    /// </summary>
    public string RedirectRoute { get; set; } = ShortLinkGeneratorConsts.DefaultRedirectRoute;
    
    /// <summary>
    /// Length of generated short codes. Default is 8.
    /// </summary>
    public int ShortCodeLength { get; set; } = ShortLinkGeneratorConsts.DefaultShortCodeLength;
    
    /// <summary>
    /// Cache expiration time in minutes for short URL lookups. Default is 60.
    /// </summary>
    public int CacheExpirationMinutes { get; set; } = ShortLinkGeneratorConsts.DefaultCacheExpirationMinutes;
    
    /// <summary>
    /// Default expiration days for new short URLs. Null means no expiration. Default is 365.
    /// </summary>
    public int? DefaultExpirationDays { get; set; } = ShortLinkGeneratorConsts.DefaultExpirationDays;
}

