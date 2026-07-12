using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.Calendar.Availability;

public class AvailabilityCalendarService : IAvailabilityCalendarService, ITransientDependency
{
    private readonly ICalendarSnapshotProvider _snapshotProvider;
    private readonly BusinessCalendarCalculator _calculator;

    public AvailabilityCalendarService(
        ICalendarSnapshotProvider snapshotProvider,
        BusinessCalendarCalculator calculator)
    {
        _snapshotProvider = snapshotProvider;
        _calculator = calculator;
    }

    public virtual async Task<bool> IsOpenAtAsync(Guid calendarId, DateTime utcInstant, CancellationToken ct = default)
    {
        return _calculator.IsOpenAt(await _snapshotProvider.GetAsync(calendarId, ct), utcInstant);
    }

    public virtual async Task<DateTime> NextOpenAtAsync(Guid calendarId, DateTime utcFrom, CancellationToken ct = default)
    {
        return _calculator.NextOpenAt(await _snapshotProvider.GetAsync(calendarId, ct), utcFrom);
    }

    public virtual async Task<DateTime> NextCloseAtAsync(Guid calendarId, DateTime utcFrom, CancellationToken ct = default)
    {
        return _calculator.NextCloseAt(await _snapshotProvider.GetAsync(calendarId, ct), utcFrom);
    }

    public virtual async Task<DateTime> AddWorkingDurationAsync(Guid calendarId, DateTime utcStart, TimeSpan working, CancellationToken ct = default)
    {
        return _calculator.AddWorkingDuration(await _snapshotProvider.GetAsync(calendarId, ct), utcStart, working);
    }

    public virtual async Task<TimeSpan> ElapsedWorkingTimeAsync(Guid calendarId, DateTime utcFrom, DateTime utcTo, CancellationToken ct = default)
    {
        return _calculator.ElapsedWorkingTime(await _snapshotProvider.GetAsync(calendarId, ct), utcFrom, utcTo);
    }
}
