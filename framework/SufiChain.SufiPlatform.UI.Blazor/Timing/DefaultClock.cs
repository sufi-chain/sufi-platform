using SufiChain.SufiPlatform.UI.Timing;

namespace SufiChain.SufiPlatform.UI.Blazor.Timing;

/// <summary>
/// Default implementation of IClock that uses UTC time.
/// </summary>
public class DefaultClock : IClock
{
    public DateTime Now => DateTime.UtcNow;

    public DateTimeKind Kind => DateTimeKind.Utc;

    public bool SupportsMultipleTimezone => false;

    public DateTime Normalize(DateTime dateTime)
    {
        if (Kind == DateTimeKind.Unspecified)
        {
            return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
        }

        if (dateTime.Kind == DateTimeKind.Unspecified)
        {
            return DateTime.SpecifyKind(dateTime, Kind);
        }

        if (dateTime.Kind != Kind)
        {
            return Kind == DateTimeKind.Utc
                ? dateTime.ToUniversalTime()
                : dateTime.ToLocalTime();
        }

        return dateTime;
    }

    public DateTime ConvertToUserTime(DateTime utcDateTime)
    {
        // Default implementation just returns UTC
        // Override in a derived class to support user timezone
        return utcDateTime;
    }

    public DateTimeOffset ConvertToUserTime(DateTimeOffset dateTimeOffset)
    {
        // Default implementation just returns as-is
        // Override in a derived class to support user timezone
        return dateTimeOffset;
    }

    public DateTime ConvertToUtc(DateTime dateTime)
    {
        if (dateTime.Kind == DateTimeKind.Utc)
        {
            return dateTime;
        }

        if (dateTime.Kind == DateTimeKind.Local)
        {
            return dateTime.ToUniversalTime();
        }

        // Unspecified - assume it's already UTC
        return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
    }
}
