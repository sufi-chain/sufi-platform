namespace SufiChain.SufiPlatform.Calendar;

public static class SufiCalendarDbProperties
{
    public static string DbTablePrefix { get; set; } = "SufiCalendar.";

    public static string? DbSchema { get; set; } = null;

    public const string ConnectionStringName = "SufiCalendar";
}