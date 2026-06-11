namespace SufiChain.SufiAbp.Calendar.FreeBusy;

public interface IFreeBusyService
{
    Task<FreeBusyResult> GetFreeBusyAsync(IReadOnlyList<Guid> calendarIds, DateTime utcFrom, DateTime utcTo, CancellationToken ct = default);
}
