using SufiChain.SufiAbp.AI;
using SufiChain.SufiAbp.Calendar.Events;

namespace SufiChain.SufiAbp.Calendar.AI.Tools;

public class CalendarCreateEventTool : CalendarAIToolBase
{
    private readonly ICalendarEventAppService _calendarEventAppService;

    public CalendarCreateEventTool(ICalendarEventAppService calendarEventAppService)
    {
        _calendarEventAppService = calendarEventAppService;
    }

    public override string Name => CalendarAIToolNames.CreateEvent;

    public override string Description => "Creates a Calendar event after Calendar event create permissions are checked by the application service.";

    public override string ParameterSchema => CalendarAIToolSchemas.CreateEvent;

    public override async Task<SufiAbpAIToolExecutionResult> ExecuteAsync(
        SufiAbpAIToolExecutionContext context,
        Dictionary<string, object?> parameters,
        CancellationToken cancellationToken = default)
    {
        var input = ReadInput<CalendarAICreateEventInput>(parameters);
        var result = await _calendarEventAppService.CreateAsync(new CreateUpdateCalendarEventDto
        {
            CalendarId = input.CalendarId,
            Title = input.Title,
            StartUtc = input.StartUtc,
            EndUtc = input.EndUtc,
            IsAllDay = input.IsAllDay,
            TimeZoneId = input.TimeZoneId,
            Location = input.Location,
            Description = input.Description,
            AvailabilityCalendarId = input.AvailabilityCalendarId,
            SourceType = input.SourceType,
            SourceId = input.SourceId
        });

        return await SuccessAsync(CalendarAIEventResult.From(result));
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

    public override string Description => "Moves a recurring event occurrence after Calendar event update permissions are checked by the application service.";

    public override string ParameterSchema => CalendarAIToolSchemas.MoveOccurrence;

    public override async Task<SufiAbpAIToolExecutionResult> ExecuteAsync(
        SufiAbpAIToolExecutionContext context,
        Dictionary<string, object?> parameters,
        CancellationToken cancellationToken = default)
    {
        var input = ReadInput<CalendarAIMoveOccurrenceInput>(parameters);
        var result = await _calendarEventAppService.MoveOccurrenceAsync(input.EventId, new MoveOccurrenceDto
        {
            OriginalStartUtc = input.OriginalStartUtc,
            MovedStartUtc = input.MovedStartUtc,
            MovedEndUtc = input.MovedEndUtc,
            ThisAndFollowing = input.ThisAndFollowing
        });

        return await SuccessAsync(CalendarAIEventResult.From(result));
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

    public override string Description => "Cancels a recurring event occurrence after Calendar event update permissions are checked by the application service.";

    public override string ParameterSchema => CalendarAIToolSchemas.CancelOccurrence;

    public override async Task<SufiAbpAIToolExecutionResult> ExecuteAsync(
        SufiAbpAIToolExecutionContext context,
        Dictionary<string, object?> parameters,
        CancellationToken cancellationToken = default)
    {
        var input = ReadInput<CalendarAICancelOccurrenceInput>(parameters);
        var result = await _calendarEventAppService.CancelOccurrenceAsync(input.EventId, new CancelOccurrenceDto
        {
            OriginalStartUtc = input.OriginalStartUtc,
            ThisAndFollowing = input.ThisAndFollowing
        });

        return await SuccessAsync(CalendarAIEventResult.From(result));
    }
}
