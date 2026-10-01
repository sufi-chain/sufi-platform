namespace SufiChain.SufiPlatform.ShortLinks.Hooshvare;

public static class ShortLinksAnalyticsHooshvareKeys
{
    public const string Key = "SufiShortLinks:Analytics";
    public const string LocalizationResourceName = "SufiShortLinks";
    public const int EntityVersion = 1;

    public static class Tools
    {
        public const string List = "shortlinks.list";
        public const string Get = "shortlinks.get";
        public const string GetAnalytics = "shortlinks.get_analytics";
    }

    public static class Context
    {
        public const string ShortUrlId = "shortUrlId";
        public const string ShortCode = "shortCode";
        public const string FromUtc = "fromUtc";
        public const string ToUtc = "toUtc";
    }

    public static class Shortcuts
    {
        public const string TopLinks = "TopLinks";
        public const string ExplainSelection = "ExplainSelection";
    }
}
