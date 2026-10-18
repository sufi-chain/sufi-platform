#if NET
using System;

namespace SufiChain.SufiPlatform.Calendar;

/// <summary>
/// Resolves an event time zone without throwing. Browsers send IANA ids (Asia/Tehran) and Windows hosts know
/// Windows ids, and a host without tzdata knows neither; any unknown id falls back instead of breaking the form
/// (sufi-chain#394).
/// </summary>
public static class CalendarTimeZoneResolver
{
    public static TimeZoneInfo Resolve(string? preferredId, string? fallbackId = null)
    {
        return TryFind(preferredId) ?? TryFind(fallbackId) ?? TimeZoneInfo.Local ?? TimeZoneInfo.Utc;
    }

    public static TimeZoneInfo? TryFind(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        if (TimeZoneInfo.TryFindSystemTimeZoneById(id, out var zone))
        {
            return zone;
        }

        if (TimeZoneInfo.TryConvertIanaIdToWindowsId(id, out var windowsId)
            && TimeZoneInfo.TryFindSystemTimeZoneById(windowsId, out zone))
        {
            return zone;
        }

        if (TimeZoneInfo.TryConvertWindowsIdToIanaId(id, out var ianaId)
            && TimeZoneInfo.TryFindSystemTimeZoneById(ianaId, out zone))
        {
            return zone;
        }

        return null;
    }
}
#endif
