namespace SufiChain.SufiPlatform.Calendar.Features;

public static class CalendarFeatures
{
    public const string GroupName = "SufiCalendar";

    public const string Assistant = "SufiCalendar.Assistant";
}

public static class CalendarAssistantChrome
{
    public static bool ShowsChrome(bool featureEnabled) => featureEnabled;

    public static bool RewritesPortalRoute(bool featureEnabled) => featureEnabled;
}
