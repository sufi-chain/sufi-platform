using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.Calendar.Availability;
using SufiChain.SufiPlatform.Calendar.Calendars;

namespace SufiChain.SufiPlatform.Calendar.AI.Tools;

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

    [SufiAiMcpTool(CalendarAIToolNames.TestAvailability, "Checks whether a Calendar is open at a UTC instant, returning the next open and close times.")]
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
    private readonly ICalendarCatalogIntegrationService _calendarCatalog;

    public CalendarListCalendarsTool(ICalendarCatalogIntegrationService calendarCatalog)
    {
        _calendarCatalog = calendarCatalog;
    }

    public override string Name => CalendarAIToolNames.ListCalendars;

    public override string Description => "Lists every calendar the user can see: personal, inherited, and shared. Returns id, name, kind (Personal, Public, Default), time zone, owner, default flag, and inheritances. Use calendar names and kinds when judging availability. Default/holiday calendars are observances. Personal and Public inherited or shared calendars occupy time. Use this first when calendarId is unknown.";

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

    [SufiAiMcpTool(CalendarAIToolNames.ListCalendars, "Lists every calendar the user can see: personal, inherited, and shared. Returns id, name, kind (Personal, Public, Default), time zone, owner, default flag, and inheritances. Use calendar names and kinds when judging availability. Default/holiday calendars are observances. Personal and Public inherited or shared calendars occupy time. Use this first when calendarId is unknown.")]
    public virtual async Task<object> ListCalendarsAsync(
        string? filter = null,
        CancellationToken cancellationToken = default)
    {
        var calendars = await _calendarCatalog.GetVisibleCalendarsAsync(filter);
        return calendars.Select(calendar => new
        {
            calendar.Id,
            calendar.Name,
            Kind = calendar.Kind.ToString(),
            calendar.TimeZoneId,
            OwnerName = calendar.OwnerName,
            calendar.IsDefault,
            Inheritances = calendar.Inheritances.Select(inheritance => new
            {
                inheritance.ParentCalendarId,
                ParentCalendarName = inheritance.ParentCalendarName,
                ParentKind = inheritance.ParentCalendarKind?.ToString(),
                inheritance.IsInheritedByDefault
            }).ToList()
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

    [SufiAiMcpTool(CalendarAIToolNames.GetWorkingHours, "Gets configured working-hour, business-hour, or opening-hour rules for a calendar. Requires calendarId; if the user gives a calendar name/title/kind or omits the id, first use calendar.list_calendars to find the best matching calendarId, preferring default calendars when the request is generic.")]
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
