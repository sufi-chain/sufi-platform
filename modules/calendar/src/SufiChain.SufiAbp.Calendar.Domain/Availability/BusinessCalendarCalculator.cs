using SufiChain.SufiAbp.Calendar.Calendars;
using SufiChain.SufiAbp.DependencyInjection;

namespace SufiChain.SufiAbp.Calendar.Availability;

public class BusinessCalendarCalculator : ITransientDependency
{
    private static readonly TimeSpan SearchStep = TimeSpan.FromMinutes(1);

    public virtual bool IsOpenAt(CalendarSnapshot snapshot, DateTime utcInstant)
    {
        if (snapshot.IsAlwaysOpen)
        {
            return true;
        }

        var localDateTime = ToLocal(snapshot, NormalizeUtc(utcInstant));
        var date = DateOnly.FromDateTime(localDateTime);
        var time = TimeOnly.FromDateTime(localDateTime);
        var exception = snapshot.Exceptions.FirstOrDefault(x => x.Date == date);

        if (exception?.Kind == CalendarExceptionKind.Closed)
        {
            return false;
        }

        var ranges = exception?.Kind == CalendarExceptionKind.SpecialHours
            ? exception.Ranges
            : snapshot.Rules.Where(x => x.DayOfWeek == localDateTime.DayOfWeek).Select(x => new WorkingHourRange(x.StartTime, x.EndTime)).ToList();

        return ranges.Any(x => x.StartTime <= time && time < x.EndTime);
    }

    public virtual DateTime NextOpenAt(CalendarSnapshot snapshot, DateTime utcFrom)
    {
        var cursor = NormalizeUtc(utcFrom);
        if (IsOpenAt(snapshot, cursor))
        {
            return cursor;
        }

        return FindBoundary(snapshot, cursor, expectedOpenState: true);
    }

    public virtual DateTime NextCloseAt(CalendarSnapshot snapshot, DateTime utcFrom)
    {
        var cursor = NormalizeUtc(utcFrom);
        if (!IsOpenAt(snapshot, cursor))
        {
            return cursor;
        }

        if (snapshot.IsAlwaysOpen)
        {
            return DateTime.MaxValue;
        }

        return FindBoundary(snapshot, cursor, expectedOpenState: false);
    }

    public virtual DateTime AddWorkingDuration(CalendarSnapshot snapshot, DateTime utcStart, TimeSpan working)
    {
        if (working < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(working));
        }

        if (working == TimeSpan.Zero)
        {
            return NormalizeUtc(utcStart);
        }

        if (snapshot.IsAlwaysOpen)
        {
            return NormalizeUtc(utcStart).Add(working);
        }

        var cursor = NextOpenAt(snapshot, utcStart);
        var remaining = working;

        while (remaining > TimeSpan.Zero)
        {
            var close = NextCloseAt(snapshot, cursor);
            var segment = close - cursor;
            if (segment >= remaining)
            {
                return cursor.Add(remaining);
            }

            remaining -= segment;
            cursor = NextOpenAt(snapshot, close.Add(SearchStep));
        }

        return cursor;
    }

    public virtual TimeSpan ElapsedWorkingTime(CalendarSnapshot snapshot, DateTime utcFrom, DateTime utcTo)
    {
        var from = NormalizeUtc(utcFrom);
        var to = NormalizeUtc(utcTo);
        if (to <= from)
        {
            return TimeSpan.Zero;
        }

        if (snapshot.IsAlwaysOpen)
        {
            return to - from;
        }

        var elapsed = TimeSpan.Zero;
        var cursor = NextOpenAt(snapshot, from);

        while (cursor < to)
        {
            var close = NextCloseAt(snapshot, cursor);
            elapsed += (close < to ? close : to) - cursor;
            cursor = NextOpenAt(snapshot, close.Add(SearchStep));
        }

        return elapsed;
    }

    protected virtual DateTime FindBoundary(CalendarSnapshot snapshot, DateTime utcFrom, bool expectedOpenState)
    {
        var cursor = utcFrom;
        var limit = utcFrom.AddYears(2);

        while (cursor < limit)
        {
            cursor = cursor.Add(SearchStep);
            if (IsOpenAt(snapshot, cursor) == expectedOpenState)
            {
                return cursor;
            }
        }

        return DateTime.MaxValue;
    }

    protected virtual DateTime ToLocal(CalendarSnapshot snapshot, DateTime utcInstant)
    {
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(snapshot.TimeZoneId);
        return TimeZoneInfo.ConvertTimeFromUtc(NormalizeUtc(utcInstant), timeZone);
    }

    protected virtual DateTime NormalizeUtc(DateTime value)
    {
        return value.Kind == DateTimeKind.Utc
            ? value
            : DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }
}
