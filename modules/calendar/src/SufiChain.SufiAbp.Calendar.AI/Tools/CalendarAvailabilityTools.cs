using SufiChain.SufiAbp.AI;
using SufiChain.SufiAbp.Calendar.Availability;

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

    public override async Task<SufiAbpAIToolExecutionResult> ExecuteAsync(
        SufiAbpAIToolExecutionContext context,
        Dictionary<string, object?> parameters,
        CancellationToken cancellationToken = default)
    {
        var input = ReadInput<CalendarAITestAvailabilityInput>(parameters);
        var result = await _availabilityCalendarAppService.TestAsync(input.CalendarId, new TestAvailabilityInput
        {
            UtcInstant = input.UtcInstant
        });

        return await SuccessAsync(new
        {
            result.IsOpen,
            result.NextOpenAtUtc,
            result.NextCloseAtUtc
        });
    }
}
