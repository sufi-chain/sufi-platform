namespace SufiChain.SufiPlatform.ShortLinks.Settings;

public static class ShortLinksSettings
{
    public const string GroupName = "SufiShortLinks";
    
    public const string BaseUrl = GroupName + ".BaseUrl";
    
    public static class ShortUrl
    {
        public const string RedirectRoute = GroupName + ".ShortUrl.RedirectRoute";
        public const string ShortCodeLength = GroupName + ".ShortUrl.ShortCodeLength";
        public const string CacheExpirationMinutes = GroupName + ".ShortUrl.CacheExpirationMinutes";
        public const string DefaultExpirationDays = GroupName + ".ShortUrl.DefaultExpirationDays";
    }
}