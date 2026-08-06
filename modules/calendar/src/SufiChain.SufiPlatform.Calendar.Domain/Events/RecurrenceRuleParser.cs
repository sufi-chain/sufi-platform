using System.Globalization;
using Volo.Abp;

namespace SufiChain.SufiPlatform.Calendar.Events;

internal static class RecurrenceRuleParser
{
    public static IReadOnlyDictionary<string, string> Parse(string rule)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var segment in rule.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var separatorIndex = segment.IndexOf('=');
            if (separatorIndex <= 0 || separatorIndex == segment.Length - 1)
            {
                throw new BusinessException(CalendarErrorCodes.InvalidRecurrenceRule);
            }

            values[segment[..separatorIndex].ToUpperInvariant()] = segment[(separatorIndex + 1)..];
        }

        return values;
    }

    public static bool IsSupportedFrequency(string frequency)
    {
        return frequency is "DAILY" or "WEEKLY" or "MONTHLY";
    }

    public static string GetRequired(this IReadOnlyDictionary<string, string> values, string key, int maxLength)
    {
        if (!values.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value) || value.Length > maxLength)
        {
            throw new BusinessException(CalendarErrorCodes.InvalidRecurrenceRule);
        }

        return value;
    }

    public static int? GetOptionalInt(this IReadOnlyDictionary<string, string> values, string key)
    {
        if (!values.TryGetValue(key, out var value))
        {
            return null;
        }

        if (!int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var number))
        {
            throw new BusinessException(CalendarErrorCodes.InvalidRecurrenceRule);
        }

        return number;
    }

    public static DateTime? GetOptionalUtc(this IReadOnlyDictionary<string, string> values, string key)
    {
        if (!values.TryGetValue(key, out var value))
        {
            return null;
        }

        if (DateTime.TryParseExact(value, "yyyyMMdd'T'HHmmss'Z'", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var utcDateTime))
        {
            return DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
        }

        if (DateTime.TryParseExact(value, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var utcDate))
        {
            return DateTime.SpecifyKind(utcDate.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);
        }

        throw new BusinessException(CalendarErrorCodes.InvalidRecurrenceRule);
    }
}
