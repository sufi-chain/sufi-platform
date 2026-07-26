using System.Linq.Expressions;

namespace SufiChain.SufiPlatform.Calendar.Events;

/// <summary>
/// Shared window filter for occurrence expansion queries.
/// Avoids loading every recurring series on a calendar (critical when inheriting busy parents).
/// </summary>
public static class CalendarEventWindowQuery
{
    /// <summary>
    /// One-shot events that overlap the window, or recurring series that could still produce
    /// occurrences in the window (started before <paramref name="toUtc"/> and not ended before
    /// <paramref name="fromUtc"/> via UNTIL). COUNT-only series without UNTIL still load when
    /// <c>StartUtc &lt; toUtc</c> — residual over-fetch is preferable to missing occurrences.
    /// </summary>
    public static Expression<Func<CalendarEvent, bool>> MatchesWindow(Guid calendarId, DateTime fromUtc, DateTime toUtc)
    {
        return x => x.CalendarId == calendarId && (
            (x.RecurrenceRule == null && x.StartUtc < toUtc && x.EndUtc > fromUtc) ||
            (x.RecurrenceRule != null &&
             x.StartUtc < toUtc &&
             (x.RecurrenceRule.UntilUtc == null || x.RecurrenceRule.UntilUtc >= fromUtc)));
    }
}
