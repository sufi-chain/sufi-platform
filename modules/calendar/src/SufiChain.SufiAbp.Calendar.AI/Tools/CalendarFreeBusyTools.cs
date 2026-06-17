using SufiChain.SufiAbp.AI;
using SufiChain.SufiAbp.Calendar.FreeBusy;

namespace SufiChain.SufiAbp.Calendar.AI.Tools;

public class CalendarGetFreeBusyTool : CalendarAIToolBase
{
    private readonly IFreeBusyAppService _freeBusyAppService;

    public CalendarGetFreeBusyTool(IFreeBusyAppService freeBusyAppService)
    {
        _freeBusyAppService = freeBusyAppService;
    }

    public override string Name => CalendarAIToolNames.GetFreeBusy;

    public override string Description => "Gets busy blocks and free slots for one or more calendars in a UTC range. Before converting relative dates, Persian dates, or local business-day ranges into UTC, call calendar.get_current_time using the selected calendar timezone.";

    public override string ParameterSchema => CalendarAIToolSchemas.FreeBusy;

    public override async Task<SufiAbpAIToolExecutionResult> ExecuteAsync(
        SufiAbpAIToolExecutionContext context,
        Dictionary<string, object?> parameters,
        CancellationToken cancellationToken = default)
    {
        var input = ReadInput<CalendarAIFreeBusyInput>(parameters);
        return await SuccessAsync(await GetFreeBusyAsync(input.CalendarIds, input.FromUtc, input.ToUtc, cancellationToken));
    }

    [SufiAbpAITool(CalendarAIToolNames.GetFreeBusy, "Gets busy blocks and free slots for one or more calendars in a UTC range. Before converting relative dates, Persian dates, or local business-day ranges into UTC, call calendar.get_current_time using the selected calendar timezone.")]
    public virtual async Task<object> GetFreeBusyAsync(
        List<Guid> calendarIds,
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default)
    {
        var result = await _freeBusyAppService.GetFreeBusyAsync(new GetFreeBusyInput
        {
            CalendarIds = calendarIds,
            FromUtc = fromUtc,
            ToUtc = toUtc
        });

        return new
        {
            result.FromUtc,
            result.ToUtc,
            BusyBlocks = result.BusyBlocks,
            FreeSlots = result.FreeSlots
        };
    }
}

public class CalendarFindFreeSlotsTool : CalendarAIToolBase
{
    private readonly IFreeBusyAppService _freeBusyAppService;

    public CalendarFindFreeSlotsTool(IFreeBusyAppService freeBusyAppService)
    {
        _freeBusyAppService = freeBusyAppService;
    }

    public override string Name => CalendarAIToolNames.FindFreeSlots;

    public override string Description => "Finds available slots for one or more calendars in a UTC range. Before converting relative dates, Persian dates, or first-working-day requests into UTC, call calendar.get_current_time using the selected calendar timezone.";

    public override string ParameterSchema => CalendarAIToolSchemas.FindFreeSlots;

    public override async Task<SufiAbpAIToolExecutionResult> ExecuteAsync(
        SufiAbpAIToolExecutionContext context,
        Dictionary<string, object?> parameters,
        CancellationToken cancellationToken = default)
    {
        var input = ReadInput<CalendarAIFindFreeSlotsInput>(parameters);
        return await SuccessAsync(await FindFreeSlotsAsync(input.CalendarIds, input.FromUtc, input.ToUtc, input.Duration, cancellationToken));
    }

    [SufiAbpAITool(CalendarAIToolNames.FindFreeSlots, "Finds available slots for one or more calendars in a UTC range. Before converting relative dates, Persian dates, or first-working-day requests into UTC, call calendar.get_current_time using the selected calendar timezone.")]
    public virtual async Task<object> FindFreeSlotsAsync(
        List<Guid> calendarIds,
        DateTime fromUtc,
        DateTime toUtc,
        TimeSpan duration,
        CancellationToken cancellationToken = default)
    {
        var result = await _freeBusyAppService.FindAvailableSlotsAsync(new FindAvailableSlotsInput
        {
            CalendarIds = calendarIds,
            FromUtc = fromUtc,
            ToUtc = toUtc,
            Duration = duration
        });

        return result.Items;
    }
}
