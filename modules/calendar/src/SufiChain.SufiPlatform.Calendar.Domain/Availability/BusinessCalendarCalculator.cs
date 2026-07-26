using SufiChain.SufiPlatform.Calendar.Calendars;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.Calendar.Availability;

public class BusinessCalendarCalculator : ITransientDependency
{
    private static readonly TimeSpan SearchStep = TimeSpan.FromMinutes(1);

    public virtual bool IsOpenAt(CalendarSnapshot snapshot, DateTime utcInstant)
    {
        if (snapshot.IsAlwaysOpen)
        {
            return true;
        }

        var normalized = NormalizeUtc(utcInstant);
        if (normalized >= DateTime.MaxValue)
        {
            return false;
        }

        var localDateTime = ToLocal(snapshot, normalized);
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

        if (!HasOpenableHours(snapshot))
        {
            return DateTime.MaxValue;
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
            return TryAdd(NormalizeUtc(utcStart), working, out var result)
                ? result
                : DateTime.MaxValue;
        }

        if (!HasOpenableHours(snapshot))
        {
            return DateTime.MaxValue;
        }

        var cursor = NextOpenAt(snapshot, utcStart);
        if (cursor >= DateTime.MaxValue)
        {
            return DateTime.MaxValue;
        }

        var remaining = working;

        while (remaining > TimeSpan.Zero)
        {
            var close = NextCloseAt(snapshot, cursor);
            if (close <= cursor || close >= DateTime.MaxValue)
            {
                return DateTime.MaxValue;
            }

            var segment = close - cursor;
            if (segment >= remaining)
            {
                return cursor.Add(remaining);
            }

            remaining -= segment;
            if (!TryAdd(close, SearchStep, out var nextFrom))
            {
                return DateTime.MaxValue;
            }

            cursor = NextOpenAt(snapshot, nextFrom);
            if (cursor >= DateTime.MaxValue)
            {
                return DateTime.MaxValue;
            }
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

        if (!HasOpenableHours(snapshot))
        {
            return TimeSpan.Zero;
        }

        var elapsed = TimeSpan.Zero;
        var cursor = NextOpenAt(snapshot, from);

        while (cursor < to && cursor < DateTime.MaxValue)
        {
            var close = NextCloseAt(snapshot, cursor);
            if (close <= cursor)
            {
                break;
            }

            elapsed += (close < to ? close : to) - cursor;
            if (!TryAdd(close, SearchStep, out var nextFrom))
            {
                break;
            }

            cursor = NextOpenAt(snapshot, nextFrom);
        }

        return elapsed;
    }

    protected virtual bool HasOpenableHours(CalendarSnapshot snapshot)
    {
        if (snapshot.IsAlwaysOpen)
        {
            return true;
        }

        if (snapshot.Rules.Count > 0)
        {
            return true;
        }

        return snapshot.Exceptions.Any(x =>
            x.Kind == CalendarExceptionKind.SpecialHours &&
            x.Ranges.Count > 0);
    }

    protected virtual DateTime FindBoundary(CalendarSnapshot snapshot, DateTime utcFrom, bool expectedOpenState)
    {
        var cursor = utcFrom;
        if (!TryAddYears(utcFrom, 2, out var limit))
        {
            limit = DateTime.MaxValue;
        }

        while (cursor < limit)
        {
            if (!TryAdd(cursor, SearchStep, out cursor))
            {
                break;
            }

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

    protected static bool TryAdd(DateTime value, TimeSpan offset, out DateTime result)
    {
        if (offset >= TimeSpan.Zero)
        {
            if (value > DateTime.MaxValue - offset)
            {
                result = DateTime.MaxValue;
                return false;
            }
        }
        else if (value < DateTime.MinValue - offset)
        {
            result = DateTime.MinValue;
            return false;
        }

        result = value.Add(offset);
        return true;
    }

    protected static bool TryAddYears(DateTime value, int years, out DateTime result)
    {
        try
        {
            result = value.AddYears(years);
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            result = DateTime.MaxValue;
            return false;
        }
    }
}
