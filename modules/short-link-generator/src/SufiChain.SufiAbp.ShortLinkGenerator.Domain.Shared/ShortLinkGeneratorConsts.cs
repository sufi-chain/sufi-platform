namespace SufiChain.SufiAbp.ShortLinkGenerator;

public static class ShortLinkGeneratorConsts
{
    public const string DefaultRedirectRoute = "short-links";
    public const int DefaultShortCodeLength = 8;
    public const int DefaultCacheExpirationMinutes = 60;
    public const int DefaultExpirationDays = 365;
    
    public static class ShortUrl
    {
        public const int MaxShortCodeLength = 20;
        public const int MaxRedirectRouteLength = 100;
        public const int MaxDestinationUrlLength = 2048;
        public const int MaxDescriptionLength = 500;
        public const int MaxCreatedByModuleLength = 100;
    }
}

