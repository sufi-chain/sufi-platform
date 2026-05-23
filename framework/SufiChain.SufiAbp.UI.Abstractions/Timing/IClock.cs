namespace SufiChain.SufiAbp.UI.Timing;

/// <summary>
/// Provides time-related operations with timezone support.
/// </summary>
public interface IClock
{
    /// <summary>
    /// Gets the current date and time.
    /// </summary>
    DateTime Now { get; }

    /// <summary>
    /// Gets the kind of DateTime returned by this clock (UTC, Local, or Unspecified).
    /// </summary>
    DateTimeKind Kind { get; }

    /// <summary>
    /// Whether this clock supports multiple timezones.
    /// </summary>
    bool SupportsMultipleTimezone { get; }

    /// <summary>
    /// Normalizes a DateTime value according to this clock's configuration.
    /// </summary>
    DateTime Normalize(DateTime dateTime);

    /// <summary>
    /// Converts a UTC DateTime to the user's timezone.
    /// </summary>
    DateTime ConvertToUserTime(DateTime utcDateTime);

    /// <summary>
    /// Converts a DateTimeOffset to the user's timezone.
    /// </summary>
    DateTimeOffset ConvertToUserTime(DateTimeOffset dateTimeOffset);

    /// <summary>
    /// Converts a DateTime to UTC.
    /// </summary>
    DateTime ConvertToUtc(DateTime dateTime);
}
