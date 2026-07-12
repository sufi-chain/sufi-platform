namespace SufiChain.SufiPlatform.Calendar.AI.Tools;

/// <summary>
/// Shared MCP tool guidance for calendar date/time handling.
/// </summary>
public static class CalendarAIToolGuidance
{
    /// <summary>
    /// Authoritative clock rule — referenced by every tool that touches dates.
    /// </summary>
    public const string SourceOfTruth =
        "Server time from calendar.get_current_time is the only source of truth for now, today, weekday, Jalali/Persian date, and Gregorian date. " +
        "Never use model training date, assumed clocks, mental Jalali/Gregorian conversion, or user-stated 'today' when it differs from that tool response.";

    /// <summary>
    /// Required first step before interpreting relative or localized dates.
    /// </summary>
    public const string MandatoryGetCurrentTime =
        "MANDATORY before any relative or localized date phrase (today, tomorrow, فردا, پس‌فردا, next Monday, دوشنبه هفته بعد, next working day, Persian month/day): " +
        "call calendar.get_current_time with the target calendarId and anchor all date math only to returned LocalDate, LocalNow, DayOfWeek, PersianDate, PersianYear, PersianMonth, PersianDay, PersianWeekdayName, GregorianDate, and TimeZoneId.";

    /// <summary>
    /// UTC parameter format rule.
    /// </summary>
    public const string UtcParameters =
        "All date-time tool parameters must be UTC ISO 8601 (e.g. 2026-06-18T09:00:00Z). " +
        "Convert local start/end using TimeZoneId from get_current_time, list_calendars, or the event — never by manual offset guessing.";

    /// <summary>
    /// Farsi / Jalali presentation rule.
    /// </summary>
    public const string FarsiAndJalali =
        "Users may ask in Farsi and use Jalali names; derive targets from get_current_time fields only. " +
        "When telling the user a date, use PersianDate/PersianWeekdayName from tool results; add GregorianDate only when the user asks.";

    /// <summary>
    /// Standard workflow prefix for scheduling tools.
    /// </summary>
    public const string SchedulingWorkflow =
        "Workflow: (1) calendar.list_calendars if calendarId/timezone unknown, (2) calendar.get_current_time, (3) derive local target date/time from tool fields, (4) convert to UTC, (5) call this tool.";

    public const string GetCurrentTime =
        "Returns authoritative server now: UtcNow, local date/time, weekday, GregorianDate, and full Jalali/Persian fields for a calendar or explicit timezone. " +
        SourceOfTruth + " " +
        "Call this first for every scheduling request — including move/reschedule/search — before computing today, tomorrow, فردا, weekdays, or Persian calendar dates. " +
        "Prefer calendarId so TimeZoneId is inherited. Never answer 'what is today' from memory; always call this tool and quote its returned fields.";

    public const string ListCalendars =
        "Lists visible calendars with id, name, kind, TimeZoneId, owner type, and default flag. " +
        "Use first when calendarId is unknown. Use returned TimeZoneId as default timezone; do not ask separately.";

    public const string GetWorkingHours =
        "Gets working-hour / business-hour rules for a calendar. Requires calendarId; if unknown, call calendar.list_calendars first.";

    public const string TestAvailability =
        "Checks whether a calendar is open at a UTC instant. " +
        MandatoryGetCurrentTime + " " +
        UtcParameters;

    public const string GetFreeBusy =
        "Gets busy blocks and free slots for calendars in a UTC range. " +
        SchedulingWorkflow + " " +
        MandatoryGetCurrentTime + " " +
        UtcParameters;

    public const string FindFreeSlots =
        "Finds available slots for calendars in a UTC range. " +
        SchedulingWorkflow + " " +
        MandatoryGetCurrentTime + " " +
        UtcParameters;

    public const string CreateEvent =
        "Creates a calendar event. " +
        SchedulingWorkflow + " " +
        MandatoryGetCurrentTime + " " +
        FarsiAndJalali + " " +
        UtcParameters + " " +
        "If calendarId is unknown, use default/first from list_calendars. If date, start, or duration is missing, ask one short question. Report success only from returned event id/times.";

    public const string SearchEvents =
        "Searches events for update/move/cancel. Call before changes when the user gives title/day/conversation reference only. " +
        MandatoryGetCurrentTime + " " +
        UtcParameters + " " +
        "Use a narrow fromUtc/toUtc range derived from get_current_time. Use titleContains when possible. If multiple matches, ask the user; never invent an eventId.";

    public const string MoveEvent =
        "Moves a non-recurring event; eventId must come from search_events or prior tool output, never guessed. " +
        MandatoryGetCurrentTime + " " +
        FarsiAndJalali + " " +
        UtcParameters + " " +
        "If identified by title/conversation, call calendar.search_events first. Preserve duration unless the user changes it. Report success only from returned event.";

    public const string MoveOccurrence =
        "Moves a recurring occurrence; eventId and originalStartUtc must come from tool results, never guessed. " +
        MandatoryGetCurrentTime + " " +
        FarsiAndJalali + " " +
        UtcParameters + " " +
        "If identified by title/conversation, call calendar.search_events first. Preserve duration unless the user changes it.";

    public const string CancelEvent =
        "Cancels an event by status; eventId must come from search_events or prior tool output, never guessed. " +
        "If identified by title/conversation, call calendar.search_events first. Reschedule only after cancel succeeds.";

    public const string CancelOccurrence =
        "Cancels a recurring occurrence; eventId and originalStartUtc must come from tool results, never guessed. " +
        "If identified by title/conversation, call calendar.search_events first. Reschedule only after cancel/move succeeds.";

    public const string UtcDateTimeParam =
        "UTC ISO 8601 with Z suffix. Compute from local date/time using TimeZoneId after calendar.get_current_time — never guess or convert Jalali/Gregorian manually.";

    public const string UtcRangeFrom =
        "UTC ISO 8601 lower bound. Derive local day/week from calendar.get_current_time, then convert that local range start to UTC.";

    public const string UtcRangeTo =
        "UTC ISO 8601 upper bound. Pair with fromUtc; keep the search range narrow.";

    public const string MovedStartUtc =
        "New start in UTC ISO 8601. Derive local target date from calendar.get_current_time, apply user's requested local time, then convert with event/calendar TimeZoneId.";

    public const string MovedEndUtc =
        "New end in UTC ISO 8601. Preserve existing duration unless the user specifies a new end or duration.";

    public const string StartUtc =
        "Event start in UTC ISO 8601. Derive local date from calendar.get_current_time and local clock time from the user, then convert with calendar TimeZoneId.";

    public const string EndUtc =
        "Event end in UTC ISO 8601. Use user's range or duration; if missing, ask one short question.";

    public const string UtcInstant =
        "UTC ISO 8601 instant to test. Derive from calendar.get_current_time when the user gives a relative or local time.";

    public const string GetCurrentTimeCalendarId =
        "Calendar id whose TimeZoneId defines 'now'. Provide for every scheduling request when a calendar is known or default.";

    public const string GetCurrentTimeTimeZoneId =
        "Explicit IANA timezone id. Use only when no calendar timezone should be inherited.";
}
