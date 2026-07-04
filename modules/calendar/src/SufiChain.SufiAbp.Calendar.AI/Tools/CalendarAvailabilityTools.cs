using SufiChain.SufiAbp.AI;
using SufiChain.SufiAbp.Calendar.Availability;
using SufiChain.SufiAbp.Calendar.Calendars;

namespace SufiChain.SufiAbp.Calendar.AI.Tools;

public class CalendarTestAvailabilityTool : CalendarAIToolBase
{
    private readonly IAvailabilityCalendarAppService _availabilityCalendarAppService;

    public CalendarTestAvailabilityTool(IAvailabilityCalendarAppService availabilityCalendarAppService)
    {
        _availabilityCalendarAppService = availabilityCalendarAppService;
    }

    public override string Name => CalendarAIToolNames.TestAvailability;

    public override string Description => "Checks whether a Calendar is open at a UTC instant, returning the next open and close times.";

    public override string ParameterSchema => CalendarAIToolSchemas.TestAvailability;

    public override async Task<SufiAIToolExecutionResult> ExecuteAsync(
        SufiAIToolExecutionContext context,
        Dictionary<string, object?> parameters,
        CancellationToken cancellationToken = default)
    {
        var input = ReadInput<CalendarAITestAvailabilityInput>(parameters);
        return await SuccessAsync(await TestAvailabilityAsync(input.CalendarId, input.UtcInstant, cancellationToken));
    }

    [SufiAITool(CalendarAIToolNames.TestAvailability, "Checks whether a Calendar is open at a UTC instant, returning the next open and close times.")]
    public virtual async Task<object> TestAvailabilityAsync(
        Guid calendarId,
        DateTime utcInstant,
        CancellationToken cancellationToken = default)
    {
        var result = await _availabilityCalendarAppService.TestAsync(calendarId, new TestAvailabilityInput
        {
            UtcInstant = utcInstant
        });

        return new
        {
            result.IsOpen,
            result.NextOpenAtUtc,
            result.NextCloseAtUtc
        };
    }
}

public class CalendarListCalendarsTool : CalendarAIToolBase
{
    private readonly IAvailabilityCalendarAppService _availabilityCalendarAppService;

    public CalendarListCalendarsTool(IAvailabilityCalendarAppService availabilityCalendarAppService)
    {
        _availabilityCalendarAppService = availabilityCalendarAppService;
    }

    public override string Name => CalendarAIToolNames.ListCalendars;

    public override string Description => "Lists visible calendars with id, name/title, kind, time zone, owner type, and default flag. Use this first to discover the correct calendarId when a user asks about calendar availability, working hours, business hours, opening hours, free/busy time, or scheduling without providing a calendar id. Use the returned TimeZoneId as the default timezone when the user says default timezone; do not ask separately.";

    public override string ParameterSchema => """
        {
          "type": "object",
          "properties": {
            "filter": { "type": "string", "description": "Optional calendar name filter." }
          }
        }
        """;

    public override async Task<SufiAIToolExecutionResult> ExecuteAsync(
        SufiAIToolExecutionContext context,
        Dictionary<string, object?> parameters,
        CancellationToken cancellationToken = default)
    {
        var input = ReadInput<CalendarAIListCalendarsInput>(parameters);
        return await SuccessAsync(await ListCalendarsAsync(input.Filter, cancellationToken));
    }

    [SufiAITool(CalendarAIToolNames.ListCalendars, "Lists visible calendars with id, name/title, kind, time zone, owner type, and default flag. Use this first to discover the correct calendarId when a user asks about calendar availability, working hours, business hours, opening hours, free/busy time, or scheduling without providing a calendar id. Use the returned TimeZoneId as the default timezone when the user says default timezone; do not ask separately.")]
    public virtual async Task<object> ListCalendarsAsync(
        string? filter = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _availabilityCalendarAppService.GetListAsync(new GetCalendarListInput
        {
            Filter = filter,
            MaxResultCount = 10
        });

        return result.Items.Select(calendar => new
        {
            calendar.Id,
            calendar.Name,
            Kind = calendar.Kind.ToString(),
            calendar.TimeZoneId,
            OwnerName = calendar.OwnerName,
            calendar.IsDefault
        }).ToList();
    }
}

public class CalendarGetWorkingHoursTool : CalendarAIToolBase
{
    private readonly IAvailabilityCalendarAppService _availabilityCalendarAppService;

    public CalendarGetWorkingHoursTool(IAvailabilityCalendarAppService availabilityCalendarAppService)
    {
        _availabilityCalendarAppService = availabilityCalendarAppService;
    }

    public override string Name => CalendarAIToolNames.GetWorkingHours;

    public override string Description => "Gets configured working-hour, business-hour, or opening-hour rules for a calendar. Requires calendarId; if the user gives a calendar name/title/kind or omits the id, first use calendar.list_calendars to find the best matching calendarId, preferring default calendars when the request is generic.";

    public override string ParameterSchema => """
        {
          "type": "object",
          "properties": {
            "calendarId": { "type": "string", "format": "uuid", "description": "Calendar id. If unknown, first call calendar.list_calendars and choose the best matching or default calendar." }
          },
          "required": [ "calendarId" ]
        }
        """;

    public override async Task<SufiAIToolExecutionResult> ExecuteAsync(
        SufiAIToolExecutionContext context,
        Dictionary<string, object?> parameters,
        CancellationToken cancellationToken = default)
    {
        var input = ReadInput<CalendarAIGetWorkingHoursInput>(parameters);
        return await SuccessAsync(await GetWorkingHoursAsync(input.CalendarId, cancellationToken));
    }

    [SufiAITool(CalendarAIToolNames.GetWorkingHours, "Gets configured working-hour, business-hour, or opening-hour rules for a calendar. Requires calendarId; if the user gives a calendar name/title/kind or omits the id, first use calendar.list_calendars to find the best matching calendarId, preferring default calendars when the request is generic.")]
    public virtual async Task<object> GetWorkingHoursAsync(
        Guid calendarId,
        CancellationToken cancellationToken = default)
    {
        var result = await _availabilityCalendarAppService.GetWorkingHoursAsync(calendarId);
        return result.Items.Select(rule => new
        {
            rule.CalendarId,
            DayOfWeek = rule.DayOfWeek.ToString(),
            StartTime = rule.StartTime.ToString(),
            EndTime = rule.EndTime.ToString()
        }).ToList();
    }
}
