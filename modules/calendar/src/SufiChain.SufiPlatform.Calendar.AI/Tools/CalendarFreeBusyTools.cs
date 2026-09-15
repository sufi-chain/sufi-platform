using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.Calendar.FreeBusy;

namespace SufiChain.SufiPlatform.Calendar.AI.Tools;

public class CalendarGetFreeBusyTool : CalendarAIToolBase
{
    private readonly IFreeBusyAppService _freeBusyAppService;

    public CalendarGetFreeBusyTool(IFreeBusyAppService freeBusyAppService)
    {
        _freeBusyAppService = freeBusyAppService;
    }

    public override string Name => CalendarAIToolNames.GetFreeBusy;

    public override string Description => "Gets busy blocks and free slots from each requested calendar's own events only. Inherited Default/Public holidays are not included. For 'am I free' questions, prefer calendar.search_events on the Personal calendar so observances can be named without treating them as personal busy time. Call calendar.get_current_time before relative or Persian dates.";

    public override string ParameterSchema => CalendarAIToolSchemas.FreeBusy;

    public override async Task<SufiAIToolExecutionResult> ExecuteAsync(
        SufiAIToolExecutionContext context,
        Dictionary<string, object?> parameters,
        CancellationToken cancellationToken = default)
    {
        var input = ReadInput<CalendarAIFreeBusyInput>(parameters);
        return await SuccessAsync(await GetFreeBusyAsync(input.CalendarIds, input.FromUtc, input.ToUtc, cancellationToken));
    }

    [SufiAiMcpTool(CalendarAIToolNames.GetFreeBusy, "Gets busy blocks and free slots from each requested calendar's own events only. Inherited Default/Public holidays are not included. For 'am I free' questions, prefer calendar.search_events on the Personal calendar so observances can be named without treating them as personal busy time. Call calendar.get_current_time before relative or Persian dates.")]
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

    public override string Description => "Finds open slots from each requested calendar's own events only. Inherited Default/Public holidays do not occupy these slots. For 'am I free' questions, also call calendar.search_events on the Personal calendar so public observances can be mentioned. Call calendar.get_current_time before relative or Persian dates.";

    public override string ParameterSchema => CalendarAIToolSchemas.FindFreeSlots;

    public override async Task<SufiAIToolExecutionResult> ExecuteAsync(
        SufiAIToolExecutionContext context,
        Dictionary<string, object?> parameters,
        CancellationToken cancellationToken = default)
    {
        var input = ReadInput<CalendarAIFindFreeSlotsInput>(parameters);
        return await SuccessAsync(await FindFreeSlotsAsync(input.CalendarIds, input.FromUtc, input.ToUtc, input.Duration, cancellationToken));
    }

    [SufiAiMcpTool(CalendarAIToolNames.FindFreeSlots, "Finds open slots from each requested calendar's own events only. Inherited Default/Public holidays do not occupy these slots. For 'am I free' questions, also call calendar.search_events on the Personal calendar so public observances can be mentioned. Call calendar.get_current_time before relative or Persian dates.")]
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
