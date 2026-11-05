using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiPlatform.Calendar;
using SufiChain.SufiPlatform.Settings;
using Volo.Abp.Settings;

namespace SufiChain.SufiPlatform.UI.Blazor.Timing;

/// <summary>
/// Display conversion for chat, tickets, and the operator console.
/// Zone ids go through <see cref="CalendarTimeZoneResolver"/> so IANA and Windows ids share one lookup
/// and this type does not keep its own offset table.
/// </summary>
public class UserTimeZoneDisplay : IUserTimeZoneDisplay
{
    /// <summary>
    /// Used when <see cref="TimingSettingNames.TimeZone"/> is missing.
    /// Tehran is UTC+3:30. A server in UTC or UTC+3 must not decide the clock.
    /// </summary>
    public const string DefaultTimeZoneId = "Asia/Tehran";

    /// <summary>
    /// The settings screen stores this word when the operator picks the default row.
    /// It is not a zone id.
    /// </summary>
    public const string UnspecifiedTimeZone = "Unspecified";

    private readonly ISettingProvider _settingProvider;

    public UserTimeZoneDisplay(ISettingProvider settingProvider)
    {
        _settingProvider = settingProvider;
    }

    public static TimeZoneInfo DefaultZone => Resolve(null);

    public async Task<TimeZoneInfo> GetAsync()
    {
        try
        {
            return Resolve(await _settingProvider.GetOrNullAsync(TimingSettingNames.TimeZone));
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return DefaultZone;
        }
    }

    /// <summary>
    /// Resolves a configured id. Null, blank, and <see cref="UnspecifiedTimeZone"/> mean Tehran.
    /// An id the host cannot find also falls back to Tehran, never to <see cref="TimeZoneInfo.Local"/>.
    /// </summary>
    public static TimeZoneInfo Resolve(string? configuredId)
    {
        var id = Normalize(configuredId);
        return CalendarTimeZoneResolver.TryFind(id)
            ?? CalendarTimeZoneResolver.TryFind(DefaultTimeZoneId)
            ?? TimeZoneInfo.Utc;
    }

    /// <summary>
    /// Converts a persisted instant to wall time in <paramref name="zone"/>.
    /// Unspecified values are UTC, which is how SQL stores them. This does not call <see cref="DateTime.ToLocalTime"/>.
    /// </summary>
    public static DateTime ToDisplayTime(DateTime value, TimeZoneInfo zone)
    {
        var utc = value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };

        return TimeZoneInfo.ConvertTimeFromUtc(utc, zone);
    }

    public static string Format(DateTime value, string format, TimeZoneInfo zone, CultureInfo? culture = null)
    {
        var display = ToDisplayTime(value, zone);
        return culture == null ? display.ToString(format) : display.ToString(format, culture);
    }

    public static async Task<TimeZoneInfo> LoadAsync(IServiceProvider services)
    {
        var display = services.GetService<IUserTimeZoneDisplay>();
        if (display == null)
        {
            return DefaultZone;
        }

        return await display.GetAsync();
    }

    private static string? Normalize(string? configuredId)
    {
        if (string.IsNullOrWhiteSpace(configuredId))
        {
            return null;
        }

        var trimmed = configuredId.Trim();
        return trimmed.Equals(UnspecifiedTimeZone, StringComparison.OrdinalIgnoreCase) ? null : trimmed;
    }
}
