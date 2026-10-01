using SufiChain.SufiPlatform.UI.Timing;

namespace SufiChain.SufiPlatform.SufiCom;

public class TestUiClock : IClock
{
    public DateTime Now => DateTime.UtcNow;

    public DateTimeKind Kind => DateTimeKind.Utc;

    public bool SupportsMultipleTimezone => false;

    public DateTime Normalize(DateTime dateTime) => dateTime.Kind switch
    {
        DateTimeKind.Local => dateTime.ToUniversalTime(),
        DateTimeKind.Unspecified => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc),
        _ => dateTime
    };

    public DateTime ConvertToUserTime(DateTime utcDateTime) => Normalize(utcDateTime);

    public DateTimeOffset ConvertToUserTime(DateTimeOffset dateTimeOffset) => dateTimeOffset.ToUniversalTime();

    public DateTime ConvertToUtc(DateTime dateTime) => Normalize(dateTime);
}
