namespace SufiChain.SufiPlatform.Calendar.Calendars;

public static class CalendarConsts
{
    public const int MaxNameLength = 128;
    public const int MaxTimeZoneIdLength = 128;
    public const int MaxOwnerNameLength = 256;
    public const int MaxDescriptionLength = 512;
    public const int MaxSourceTypeLength = 128;
    public const int MaxSourceIdLength = 64;
    public const int MaxColorLength = 32;
    public const string DefaultColor = "#2563eb";

    public const string HostHijriCalendarName = "Hijri Shamsi";

    public static string GetDefaultColor(CalendarKind kind)
    {
        return kind switch
        {
            CalendarKind.Personal => "#0f766e",
            CalendarKind.Public => "#2563eb",
            CalendarKind.Default => "#d97706",
            _ => DefaultColor
        };
    }
}
