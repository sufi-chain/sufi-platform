using Volo.Abp;
using SufiChain.SufiAbp.Calendar.Events;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiAbp.Calendar.Scheduling;

public class RecurrenceCalculator : ITransientDependency
{
    public virtual IReadOnlyList<EventOccurrence> Expand(CalendarEvent calendarEvent, DateTime windowStartUtc, DateTime windowEndUtc)
    {
        if (windowEndUtc <= windowStartUtc || (windowEndUtc - windowStartUtc).TotalDays > EventConsts.MaxExpansionWindowDays)
        {
            throw new BusinessException(CalendarErrorCodes.InvalidExpansionWindow);
        }

        if (calendarEvent.RecurrenceRule is null)
        {
            return Overlaps(calendarEvent.StartUtc, calendarEvent.EndUtc, windowStartUtc, windowEndUtc)
                ? new[] { CreateOccurrence(calendarEvent, calendarEvent.StartUtc, calendarEvent.StartUtc, calendarEvent.EndUtc) }
                : Array.Empty<EventOccurrence>();
        }

        return ExpandRecurring(calendarEvent, windowStartUtc, windowEndUtc);
    }

    private static IReadOnlyList<EventOccurrence> ExpandRecurring(CalendarEvent calendarEvent, DateTime windowStartUtc, DateTime windowEndUtc)
    {
        var rule = calendarEvent.RecurrenceRule!;
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(calendarEvent.TimeZoneId);
        var duration = calendarEvent.EndUtc - calendarEvent.StartUtc;
        var firstLocalStart = TimeZoneInfo.ConvertTimeFromUtc(calendarEvent.StartUtc, timeZone);
        var occurrences = new List<EventOccurrence>();
        var generatedCount = 0;
        var candidateLocalStart = firstLocalStart;
        var exactExceptions = calendarEvent.OccurrenceExceptions
            .Where(x => !x.ThisAndFollowing)
            .ToDictionary(x => x.OriginalStartUtc, x => x);
        var thisAndFollowingException = calendarEvent.OccurrenceExceptions
            .Where(x => x.ThisAndFollowing)
            .OrderBy(x => x.OriginalStartUtc)
            .FirstOrDefault();

        while (generatedCount < (rule.Count ?? int.MaxValue))
        {
            var candidateStartUtc = ToUtc(candidateLocalStart, timeZone);
            if (rule.UntilUtc.HasValue && candidateStartUtc > rule.UntilUtc.Value)
            {
                break;
            }

            if (candidateStartUtc >= windowEndUtc)
            {
                break;
            }

            var candidateEndUtc = candidateStartUtc.Add(duration);
            generatedCount++;

            if (candidateEndUtc > windowStartUtc)
            {
                var occurrence = ApplyException(calendarEvent, candidateStartUtc, candidateEndUtc, exactExceptions, thisAndFollowingException);
                if (occurrence is not null && Overlaps(occurrence.StartUtc, occurrence.EndUtc, windowStartUtc, windowEndUtc))
                {
                    occurrences.Add(occurrence);
                }
            }

            candidateLocalStart = NextLocalStart(candidateLocalStart, rule.Frequency, rule.Interval);
        }

        return occurrences.OrderBy(x => x.StartUtc).ThenBy(x => x.EndUtc).ToList();
    }

    private static EventOccurrence? ApplyException(
        CalendarEvent calendarEvent,
        DateTime originalStartUtc,
        DateTime originalEndUtc,
        IReadOnlyDictionary<DateTime, EventOccurrenceException> exactExceptions,
        EventOccurrenceException? thisAndFollowingException)
    {
        if (exactExceptions.TryGetValue(originalStartUtc, out var exactException))
        {
            return CreateOccurrenceOrNull(calendarEvent, originalStartUtc, originalEndUtc, exactException);
        }

        if (thisAndFollowingException is not null && originalStartUtc >= thisAndFollowingException.OriginalStartUtc)
        {
            return CreateOccurrenceOrNull(calendarEvent, originalStartUtc, originalEndUtc, thisAndFollowingException);
        }

        return CreateOccurrence(calendarEvent, originalStartUtc, originalStartUtc, originalEndUtc);
    }

    private static EventOccurrence? CreateOccurrenceOrNull(CalendarEvent calendarEvent, DateTime originalStartUtc, DateTime originalEndUtc, EventOccurrenceException exception)
    {
        if (exception.IsCancelled)
        {
            return null;
        }

        return CreateOccurrence(calendarEvent, originalStartUtc, exception.OverrideStartUtc ?? originalStartUtc, exception.OverrideEndUtc ?? originalEndUtc);
    }

    private static EventOccurrence CreateOccurrence(CalendarEvent calendarEvent, DateTime originalStartUtc, DateTime startUtc, DateTime endUtc)
    {
        return new EventOccurrence(
            calendarEvent.Id,
            calendarEvent.CalendarId,
            calendarEvent.Title,
            originalStartUtc,
            startUtc,
            endUtc,
            calendarEvent.IsAllDay,
            calendarEvent.TimeZoneId,
            calendarEvent.Status,
            calendarEvent.Location,
            calendarEvent.Description,
            calendarEvent.Color,
            calendarEvent.SourceType,
            calendarEvent.SourceId);
    }

    private static DateTime NextLocalStart(DateTime localStart, string frequency, int interval)
    {
        return frequency switch
        {
            "DAILY" => localStart.AddDays(interval),
            "WEEKLY" => localStart.AddDays(7 * interval),
            "MONTHLY" => localStart.AddMonths(interval),
            _ => throw new BusinessException(CalendarErrorCodes.InvalidRecurrenceRule)
        };
    }

    private static DateTime ToUtc(DateTime localDateTime, TimeZoneInfo timeZone)
    {
        var unspecified = DateTime.SpecifyKind(localDateTime, DateTimeKind.Unspecified);
        if (timeZone.IsInvalidTime(unspecified))
        {
            unspecified = unspecified.AddHours(1);
        }

        if (timeZone.IsAmbiguousTime(unspecified))
        {
            return DateTime.SpecifyKind(TimeZoneInfo.ConvertTimeToUtc(unspecified, timeZone), DateTimeKind.Utc);
        }

        return DateTime.SpecifyKind(TimeZoneInfo.ConvertTimeToUtc(unspecified, timeZone), DateTimeKind.Utc);
    }

    private static bool Overlaps(DateTime startUtc, DateTime endUtc, DateTime windowStartUtc, DateTime windowEndUtc)
    {
        return startUtc < windowEndUtc && endUtc > windowStartUtc;
    }
}
