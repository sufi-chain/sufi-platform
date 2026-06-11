using Microsoft.AspNetCore.Mvc;
using SufiChain.SufiAbp.Application.Dtos;
using SufiChain.SufiAbp.Calendar.FreeBusy;
using Volo.Abp;

namespace SufiChain.SufiAbp.Calendar.Controllers;

[Area(CalendarRemoteServiceConsts.ModuleName)]
[RemoteService(Name = CalendarRemoteServiceConsts.RemoteServiceName)]
[Route("api/calendar/free-busy")]
public class FreeBusyController : CalendarController, IFreeBusyAppService
{
    private readonly IFreeBusyAppService _freeBusyAppService;

    public FreeBusyController(IFreeBusyAppService freeBusyAppService)
    {
        _freeBusyAppService = freeBusyAppService;
    }

    [HttpPost]
    public virtual Task<FreeBusyResultDto> GetFreeBusyAsync(GetFreeBusyInput input) => _freeBusyAppService.GetFreeBusyAsync(input);

    [HttpPost]
    [Route("available-slots")]
    public virtual Task<ListResultDto<FreeBusySlotDto>> FindAvailableSlotsAsync(FindAvailableSlotsInput input) => _freeBusyAppService.FindAvailableSlotsAsync(input);
}
