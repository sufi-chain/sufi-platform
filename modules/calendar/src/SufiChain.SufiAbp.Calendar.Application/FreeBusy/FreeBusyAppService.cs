using SufiChain.SufiAbp.Application.Dtos;
using SufiChain.SufiAbp.Application.Services;
using SufiChain.SufiAbp.Calendar.Permissions;

namespace SufiChain.SufiAbp.Calendar.FreeBusy;

public class FreeBusyAppService : SufiAbpApplicationService, IFreeBusyAppService
{
    private readonly IFreeBusyService _freeBusyService;

    public FreeBusyAppService(IFreeBusyService freeBusyService)
    {
        _freeBusyService = freeBusyService;
    }

    public virtual async Task<FreeBusyResultDto> GetFreeBusyAsync(GetFreeBusyInput input)
    {
        await CheckPolicyAsync(CalendarPermissions.Events.Default);
        return FreeBusyDtoMapper.ToDto(await _freeBusyService.GetFreeBusyAsync(input.CalendarIds, input.FromUtc, input.ToUtc));
    }

    public virtual async Task<ListResultDto<FreeBusySlotDto>> FindAvailableSlotsAsync(FindAvailableSlotsInput input)
    {
        await CheckPolicyAsync(CalendarPermissions.Events.Default);
        var result = await _freeBusyService.GetFreeBusyAsync(input.CalendarIds, input.FromUtc, input.ToUtc);
        var slots = result.FreeSlots
            .Where(x => x.EndUtc - x.StartUtc >= input.Duration)
            .Select(FreeBusyDtoMapper.ToDto)
            .ToList();
        return new ListResultDto<FreeBusySlotDto>(slots);
    }
}
