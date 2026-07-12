  using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.Calendar.Events;

namespace SufiChain.SufiPlatform.Calendar.AI.Tools;

public class CalendarCreateEventTool : CalendarAIToolBase
{
    private readonly ICalendarEventAppService _calendarEventAppService;

    public CalendarCreateEventTool(ICalendarEventAppService calendarEventAppService)
    {
        _calendarEventAppService = calendarEventAppService;
    }

    public override string Name => CalendarAIToolNames.CreateEvent;

    public override string Description => "Creates a calendar event after resolving only the data required for scheduling. Users may ask in Farsi; when they do, interpret Farsi relative-date words and report dates in the Jalali calendar unless the user asks otherwise. If calendarId is unknown, call calendar.list_calendars and use the default or first matching calendar when the user says personal/default/first calendar. If the user says default timezone, inherit the selected calendar TimeZoneId; do not ask a separate timezone question. Before interpreting relative dates, Persian calendar dates, or next working-day requests, call calendar.get_current_time. A title may be formed from the user's subject. If date, start, or end/duration is missing, ask one short natural question. Convert local times to UTC before calling. Report success only from the returned event id/times.";

    public override string ParameterSchema => CalendarAIToolSchemas.CreateEvent;

    public override async Task<SufiAIToolExecutionResult> ExecuteAsync(
        SufiAIToolExecutionContext context,
        Dictionary<string, object?> parameters,
        CancellationToken cancellationToken = default)
    {
        var input = ReadInput<CalendarAICreateEventInput>(parameters);
        return await SuccessAsync(await CreateEventAsync(
            input.CalendarId,
            input.Title,
            input.StartUtc,
            input.EndUtc,
            input.IsAllDay,
            input.TimeZoneId,
            input.Location,
            input.Description,
            input.AvailabilityCalendarId,
            input.SourceType,
            input.SourceId,
            cancellationToken));
    }

    [SufiAITool(CalendarAIToolNames.CreateEvent, "Creates a calendar event after resolving only the data required for scheduling. Users may ask in Farsi; when they do, interpret Farsi relative-date words and report dates in the Jalali calendar unless the user asks otherwise. If calendarId is unknown, call calendar.list_calendars and use the default or first matching calendar when the user says personal/default/first calendar. If the user says default timezone, inherit the selected calendar TimeZoneId; do not ask a separate timezone question. Before interpreting relative dates, Persian calendar dates, or next working-day requests, call calendar.get_current_time. A title may be formed from the user's subject. If date, start, or end/duration is missing, ask one short natural question. Convert local times to UTC before calling. Report success only from the returned event id/times.")]
    public virtual async Task<CalendarAIEventResult> CreateEventAsync(
        Guid calendarId,
        string title,
        DateTime startUtc,
        DateTime endUtc,
        bool isAllDay = false,
        string timeZoneId = "UTC",
        string? location = null,
        string? description = null,
        Guid? availabilityCalendarId = null,
        string? sourceType = null,
        string? sourceId = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _calendarEventAppService.CreateAsync(new CreateUpdateCalendarEventDto
        {
            CalendarId = calendarId,
            Title = title,
            StartUtc = startUtc,
            EndUtc = endUtc,
            IsAllDay = isAllDay,
            TimeZoneId = timeZoneId,
            Location = location,
            Description = description,
            AvailabilityCalendarId = availabilityCalendarId,
            SourceType = sourceType,
            SourceId = sourceId
        });

        return CalendarAIEventResult.From(result);
    }
}

public class CalendarSearchEventsTool : CalendarAIToolBase
{
    private readonly ICalendarEventAppService _calendarEventAppService;

    public CalendarSearchEventsTool(ICalendarEventAppService calendarEventAppService)
    {
        _calendarEventAppService = calendarEventAppService;
    }

    public override string Name => CalendarAIToolNames.SearchEvents;

    public override string Description => "Searches existing calendar events and returns event ids needed for update, move, cancel, or delete requests. Use this before changing an event when the user gives only a title, day, or conversational reference. Before converting relative dates or Persian dates into fromUtc/toUtc, call calendar.get_current_time using the selected calendar timezone. Prefer a narrow date range from the conversation and a titleContains filter. If multiple likely matches are returned, ask the user to choose; do not invent an event id.";

    public override string ParameterSchema => CalendarAIToolSchemas.SearchEvents;

    public override async Task<SufiAIToolExecutionResult> ExecuteAsync(
        SufiAIToolExecutionContext context,
        Dictionary<string, object?> parameters,
        CancellationToken cancellationToken = default)
    {
        var input = ReadInput<CalendarAISearchEventsInput>(parameters);
        return await SuccessAsync(await SearchEventsAsync(
            input.CalendarId,
            input.FromUtc,
            input.ToUtc,
            input.TitleContains,
            input.MaxResultCount,
            cancellationToken));
    }

    [SufiAITool(CalendarAIToolNames.SearchEvents, "Searches existing calendar events and returns event ids needed for update, move, cancel, or delete requests. Use this before changing an event when the user gives only a title, day, or conversational reference. Before converting relative dates or Persian dates into fromUtc/toUtc, call calendar.get_current_time using the selected calendar timezone. Prefer a narrow date range from the conversation and a titleContains filter. If multiple likely matches are returned, ask the user to choose; do not invent an event id.")]
    public virtual async Task<object> SearchEventsAsync(
        Guid? calendarId = null,
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        string? titleContains = null,
        int maxResultCount = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _calendarEventAppService.GetListAsync(new GetEventListInput
        {
            CalendarId = calendarId,
            FromUtc = fromUtc,
            ToUtc = toUtc,
            MaxResultCount = Math.Clamp(maxResultCount, 1, 20)
        });

        var events = result.Items.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(titleContains))
        {
            events = events.Where(calendarEvent =>
                calendarEvent.Title.Contains(titleContains, StringComparison.OrdinalIgnoreCase));
        }

        return events.Select(calendarEvent => new
        {
            calendarEvent.Id,
            calendarEvent.CalendarId,
            calendarEvent.Title,
            calendarEvent.StartUtc,
            calendarEvent.EndUtc,
            calendarEvent.TimeZoneId,
            Status = calendarEvent.Status.ToString(),
            calendarEvent.Location
        }).ToList();
    }
}

public class CalendarMoveEventTool : CalendarAIToolBase
{
    private readonly ICalendarEventAppService _calendarEventAppService;

    public CalendarMoveEventTool(ICalendarEventAppService calendarEventAppService)
    {
        _calendarEventAppService = calendarEventAppService;
    }

    public override string Name => CalendarAIToolNames.MoveEvent;

    public override string Description => "Moves a normal non-recurring calendar event only when eventId is known from tool results, not guesses. Users may ask in Farsi; when they do, interpret Farsi relative-date words and report dates in the Jalali calendar unless the user asks otherwise. If the user identifies the event by title or conversation, first call calendar.search_events. Before interpreting relative dates or next working-day requests, call calendar.get_current_time. Preserve title, timezone, calendar, status, and other details; preserve the original duration unless the user asks otherwise. Convert requested local move time to UTC using the event or calendar timezone. Report success only from the returned event.";

    public override string ParameterSchema => CalendarAIToolSchemas.MoveEvent;

    public override async Task<SufiAIToolExecutionResult> ExecuteAsync(
        SufiAIToolExecutionContext context,
        Dictionary<string, object?> parameters,
        CancellationToken cancellationToken = default)
    {
        var input = ReadInput<CalendarAIMoveEventInput>(parameters);
        return await SuccessAsync(await MoveEventAsync(input.EventId, input.MovedStartUtc, input.MovedEndUtc, cancellationToken));
    }

    [SufiAITool(CalendarAIToolNames.MoveEvent, "Moves a normal non-recurring calendar event only when eventId is known from tool results, not guesses. Users may ask in Farsi; when they do, interpret Farsi relative-date words and report dates in the Jalali calendar unless the user asks otherwise. If the user identifies the event by title or conversation, first call calendar.search_events. Before interpreting relative dates or next working-day requests, call calendar.get_current_time. Preserve title, timezone, calendar, status, and other details; preserve the original duration unless the user asks otherwise. Convert requested local move time to UTC using the event or calendar timezone. Report success only from the returned event.")]
    public virtual async Task<CalendarAIEventResult> MoveEventAsync(
        Guid eventId,
        DateTime movedStartUtc,
        DateTime movedEndUtc,
        CancellationToken cancellationToken = default)
    {
        var existing = await _calendarEventAppService.GetAsync(eventId);
        var result = await _calendarEventAppService.UpdateAsync(eventId, new CreateUpdateCalendarEventDto
        {
            CalendarId = existing.CalendarId,
            Title = existing.Title,
            StartUtc = movedStartUtc,
            EndUtc = movedEndUtc,
            IsAllDay = existing.IsAllDay,
            TimeZoneId = existing.TimeZoneId,
            Location = existing.Location,
            Description = existing.Description,
            Color = existing.Color,
            Status = existing.Status,
            AvailabilityCalendarId = existing.AvailabilityCalendarId,
            SourceType = existing.SourceType,
            SourceId = existing.SourceId,
            RecurrenceRule = existing.RecurrenceRule,
            ExtraProperties = existing.ExtraProperties
        });

        return CalendarAIEventResult.From(result);
    }
}

public class CalendarCancelEventTool : CalendarAIToolBase
{
    private readonly ICalendarEventAppService _calendarEventAppService;

    public CalendarCancelEventTool(ICalendarEventAppService calendarEventAppService)
    {
        _calendarEventAppService = calendarEventAppService;
    }

    public override string Name => CalendarAIToolNames.CancelEvent;

    public override string Description => "Cancels a normal calendar event by setting its status to Cancelled only when eventId is known from tool results, not guesses. If the user identifies the event by title or conversation, first call calendar.search_events. If the user asks to reschedule, create or move the replacement only after this tool returns success. Report success only from returned tool data.";

    public override string ParameterSchema => CalendarAIToolSchemas.CancelEvent;

    public override async Task<SufiAIToolExecutionResult> ExecuteAsync(
        SufiAIToolExecutionContext context,
        Dictionary<string, object?> parameters,
        CancellationToken cancellationToken = default)
    {
        var input = ReadInput<CalendarAICancelEventInput>(parameters);
        return await SuccessAsync(await CancelEventAsync(input.EventId, cancellationToken));
    }

    [SufiAITool(CalendarAIToolNames.CancelEvent, "Cancels a normal calendar event by setting its status to Cancelled only when eventId is known from tool results, not guesses. If the user identifies the event by title or conversation, first call calendar.search_events. If the user asks to reschedule, create or move the replacement only after this tool returns success. Report success only from returned tool data.")]
    public virtual async Task<CalendarAIEventResult> CancelEventAsync(
        Guid eventId,
        CancellationToken cancellationToken = default)
    {
        var existing = await _calendarEventAppService.GetAsync(eventId);
        var result = await _calendarEventAppService.UpdateAsync(eventId, new CreateUpdateCalendarEventDto
        {
            CalendarId = existing.CalendarId,
            Title = existing.Title,
            StartUtc = existing.StartUtc,
            EndUtc = existing.EndUtc,
            IsAllDay = existing.IsAllDay,
            TimeZoneId = existing.TimeZoneId,
            Location = existing.Location,
            Description = existing.Description,
            Color = existing.Color,
            Status = EventStatus.Cancelled,
            AvailabilityCalendarId = existing.AvailabilityCalendarId,
            SourceType = existing.SourceType,
            SourceId = existing.SourceId,
            RecurrenceRule = existing.RecurrenceRule,
            ExtraProperties = existing.ExtraProperties
        });

        return CalendarAIEventResult.From(result);
    }
}

public class CalendarMoveOccurrenceTool : CalendarAIToolBase
{
    private readonly ICalendarEventAppService _calendarEventAppService;

    public CalendarMoveOccurrenceTool(ICalendarEventAppService calendarEventAppService)
    {
        _calendarEventAppService = calendarEventAppService;
    }

    public override string Name => CalendarAIToolNames.MoveOccurrence;

    public override string Description => "Moves a recurring event occurrence only when eventId and originalStartUtc are known from tool results, not guesses. Users may ask in Farsi; when they do, interpret Farsi relative-date words and report dates in the Jalali calendar unless the user asks otherwise. If the user identifies the event by title or conversation, first call calendar.search_events. Before interpreting relative dates or next working-day requests, call calendar.get_current_time. Preserve the original duration unless the user asks otherwise. Convert requested local move time to UTC using the event or calendar timezone. Report success only from the returned event.";

    public override string ParameterSchema => CalendarAIToolSchemas.MoveOccurrence;

    public override async Task<SufiAIToolExecutionResult> ExecuteAsync(
        SufiAIToolExecutionContext context,
        Dictionary<string, object?> parameters,
        CancellationToken cancellationToken = default)
    {
        var input = ReadInput<CalendarAIMoveOccurrenceInput>(parameters);
        return await SuccessAsync(await MoveOccurrenceAsync(
            input.EventId,
            input.OriginalStartUtc,
            input.MovedStartUtc,
            input.MovedEndUtc,
            input.ThisAndFollowing,
            cancellationToken));
    }

    [SufiAITool(CalendarAIToolNames.MoveOccurrence, "Moves a recurring event occurrence only when eventId and originalStartUtc are known from tool results, not guesses. Users may ask in Farsi; when they do, interpret Farsi relative-date words and report dates in the Jalali calendar unless the user asks otherwise. If the user identifies the event by title or conversation, first call calendar.search_events. Before interpreting relative dates or next working-day requests, call calendar.get_current_time. Preserve the original duration unless the user asks otherwise. Convert requested local move time to UTC using the event or calendar timezone. Report success only from the returned event.")]
    public virtual async Task<CalendarAIEventResult> MoveOccurrenceAsync(
        Guid eventId,
        DateTime originalStartUtc,
        DateTime movedStartUtc,
        DateTime movedEndUtc,
        bool thisAndFollowing = false,
        CancellationToken cancellationToken = default)
    {
        var result = await _calendarEventAppService.MoveOccurrenceAsync(eventId, new MoveOccurrenceDto
        {
            OriginalStartUtc = originalStartUtc,
            MovedStartUtc = movedStartUtc,
            MovedEndUtc = movedEndUtc,
            ThisAndFollowing = thisAndFollowing
        });

        return CalendarAIEventResult.From(result);
    }
}

public class CalendarCancelOccurrenceTool : CalendarAIToolBase
{
    private readonly ICalendarEventAppService _calendarEventAppService;

    public CalendarCancelOccurrenceTool(ICalendarEventAppService calendarEventAppService)
    {
        _calendarEventAppService = calendarEventAppService;
    }

    public override string Name => CalendarAIToolNames.CancelOccurrence;

    public override string Description => "Cancels a recurring event occurrence only when eventId and originalStartUtc are known from tool results, not guesses. If the user identifies the event by title or conversation, first call calendar.search_events. If the user wants to reschedule, create or move the replacement only after the cancel/move tool returns success. Report success only from returned tool data.";

    public override string ParameterSchema => CalendarAIToolSchemas.CancelOccurrence;

    public override async Task<SufiAIToolExecutionResult> ExecuteAsync(
        SufiAIToolExecutionContext context,
        Dictionary<string, object?> parameters,
        CancellationToken cancellationToken = default)
    {
        var input = ReadInput<CalendarAICancelOccurrenceInput>(parameters);
        return await SuccessAsync(await CancelOccurrenceAsync(
            input.EventId,
            input.OriginalStartUtc,
            input.ThisAndFollowing,
            cancellationToken));
    }

    [SufiAITool(CalendarAIToolNames.CancelOccurrence, "Cancels a recurring event occurrence only when eventId and originalStartUtc are known from tool results, not guesses. If the user identifies the event by title or conversation, first call calendar.search_events. If the user wants to reschedule, create or move the replacement only after the cancel/move tool returns success. Report success only from returned tool data.")]
    public virtual async Task<CalendarAIEventResult> CancelOccurrenceAsync(
        Guid eventId,
        DateTime originalStartUtc,
        bool thisAndFollowing = false,
        CancellationToken cancellationToken = default)
    {
        var result = await _calendarEventAppService.CancelOccurrenceAsync(eventId, new CancelOccurrenceDto
        {
            OriginalStartUtc = originalStartUtc,
            ThisAndFollowing = thisAndFollowing
        });

        return CalendarAIEventResult.From(result);
    }
}
