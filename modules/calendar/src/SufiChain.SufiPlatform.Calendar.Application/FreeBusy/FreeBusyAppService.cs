using SufiChain.SufiPlatform.Application.Dtos;
using SufiChain.SufiPlatform.Application.Services;
using SufiChain.SufiPlatform.Calendar.Permissions;

namespace SufiChain.SufiPlatform.Calendar.FreeBusy;

public class FreeBusyAppService : SufiApplicationService, IFreeBusyAppService
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
