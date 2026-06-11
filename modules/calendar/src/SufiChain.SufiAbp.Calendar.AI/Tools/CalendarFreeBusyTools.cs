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

    public override string Description => "Gets busy blocks and free slots for one or more calendars in a UTC range.";

    public override string ParameterSchema => CalendarAIToolSchemas.FreeBusy;

    public override async Task<SufiAbpAIToolExecutionResult> ExecuteAsync(
        SufiAbpAIToolExecutionContext context,
        Dictionary<string, object?> parameters,
        CancellationToken cancellationToken = default)
    {
        var input = ReadInput<CalendarAIFreeBusyInput>(parameters);
        var result = await _freeBusyAppService.GetFreeBusyAsync(new GetFreeBusyInput
        {
            CalendarIds = input.CalendarIds,
            FromUtc = input.FromUtc,
            ToUtc = input.ToUtc
        });

        return await SuccessAsync(new
        {
            result.FromUtc,
            result.ToUtc,
            BusyBlocks = result.BusyBlocks,
            FreeSlots = result.FreeSlots
        });
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

    public override string Description => "Finds available slots for one or more calendars in a UTC range.";

    public override string ParameterSchema => CalendarAIToolSchemas.FindFreeSlots;

    public override async Task<SufiAbpAIToolExecutionResult> ExecuteAsync(
        SufiAbpAIToolExecutionContext context,
        Dictionary<string, object?> parameters,
        CancellationToken cancellationToken = default)
    {
        var input = ReadInput<CalendarAIFindFreeSlotsInput>(parameters);
        var result = await _freeBusyAppService.FindAvailableSlotsAsync(new FindAvailableSlotsInput
        {
            CalendarIds = input.CalendarIds,
            FromUtc = input.FromUtc,
            ToUtc = input.ToUtc,
            Duration = input.Duration
        });

        return await SuccessAsync(result.Items);
    }
}
