using SufiChain.SufiAbp.Domain.Entities;
using SufiChain.SufiAbp;

namespace SufiChain.SufiAbp.Calendar.Calendars;

public class CalendarException : Entity<Guid>
{
    public virtual Guid CalendarId { get; private set; }

    public virtual DateOnly Date { get; private set; }

    public virtual CalendarExceptionKind Kind { get; private set; }

    public virtual string? Description { get; private set; }

    public virtual List<WorkingHourRange> Ranges { get; private set; } = new();

    protected CalendarException()
    {
    }

    public CalendarException(Guid id, Guid calendarId, DateOnly date, CalendarExceptionKind kind, IEnumerable<WorkingHourRange>? ranges = null, string? description = null)
        : base(id)
    {
        CalendarId = calendarId;
        Date = date;
        Kind = kind;
        Description = description;
        ReplaceRanges(ranges ?? Array.Empty<WorkingHourRange>());
    }

    public virtual void ReplaceRanges(IEnumerable<WorkingHourRange> ranges)
    {
        var rangeList = ranges.ToList();

        if (Kind == CalendarExceptionKind.Closed && rangeList.Count > 0)
        {
            throw new BusinessException(CalendarErrorCodes.InvalidTimeRange);
        }

        EnsureNoOverlaps(rangeList, CalendarErrorCodes.OverlappingExceptionHours);

        Ranges.Clear();
        Ranges.AddRange(rangeList);
    }

    public virtual void SetDescription(string? description)
    {
        Description = description;
    }

    internal static void EnsureNoOverlaps(IReadOnlyCollection<WorkingHourRange> ranges, string errorCode)
    {
        foreach (var range in ranges)
        {
            if (range.EndTime <= range.StartTime)
            {
                throw new BusinessException(CalendarErrorCodes.InvalidTimeRange);
            }

            if (range.MaxConcurrent is <= 0)
            {
                throw new BusinessException(CalendarErrorCodes.InvalidMaxConcurrent);
            }
        }

        var orderedRanges = ranges.OrderBy(x => x.StartTime).ToList();
        for (var index = 1; index < orderedRanges.Count; index++)
        {
            if (orderedRanges[index].StartTime < orderedRanges[index - 1].EndTime)
            {
                throw new BusinessException(errorCode);
            }
        }
    }
}
