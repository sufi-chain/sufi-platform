namespace SufiChain.SufiAbp.ShortLinkGenerator.Settings;

public static class ShortLinkGeneratorSettings
{
    public const string GroupName = "ShortLinkGenerator";
    
    public const string BaseUrl = GroupName + ".BaseUrl";
    
    public static class ShortUrl
    {
        public const string RedirectRoute = GroupName + ".ShortUrl.RedirectRoute";
        public const string ShortCodeLength = GroupName + ".ShortUrl.ShortCodeLength";
        public const string CacheExpirationMinutes = GroupName + ".ShortUrl.CacheExpirationMinutes";
        public const string DefaultExpirationDays = GroupName + ".ShortUrl.DefaultExpirationDays";
    }
}

