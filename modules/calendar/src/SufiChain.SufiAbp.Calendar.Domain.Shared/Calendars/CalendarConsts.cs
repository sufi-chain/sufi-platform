namespace SufiChain.SufiAbp.Calendar.Calendars;

public static class CalendarConsts
{
    public const string DbTablePrefix = "Calendar.";
    public const string? DbSchema = null;
    public const string ConnectionStringName = "Calendar";

    public const int MaxNameLength = 128;
    public const int MaxTimeZoneIdLength = 128;
    public const int MaxOwnerNameLength = 256;
    public const int MaxDescriptionLength = 512;
    public const int MaxSourceTypeLength = 128;
    public const int MaxSourceIdLength = 64;

    public const string HostHijriCalendarName = "Hijri Shamsi 1405";
}
