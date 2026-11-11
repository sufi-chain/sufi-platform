#if NET
namespace SufiChain.SufiPlatform.Calendar;

/// <summary>
/// Picks the zone id to store. A picked id that the host understands wins.
/// An empty or unknown id uses the ABP timing setting (<c>Abp.Timing.TimeZone</c>), then UTC.
/// </summary>
public static class CalendarTiming
{
    public const string SettingName = "Abp.Timing.TimeZone";

    public const string UnspecifiedTimeZone = "Unspecified";

    public static string ResolveId(string? requested, string? configuredSetting)
    {
        if (CalendarTimeZoneResolver.TryFind(Normalize(requested)) is { } requestedZone)
        {
            return requestedZone.Id;
        }

        if (CalendarTimeZoneResolver.TryFind(Normalize(configuredSetting)) is { } configuredZone)
        {
            return configuredZone.Id;
        }

        return CalendarTimeZoneResolver.TryFind("UTC")?.Id ?? "UTC";
    }

    private static string? Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        return trimmed.Equals(UnspecifiedTimeZone, StringComparison.OrdinalIgnoreCase) ? null : trimmed;
    }
}
#endif
