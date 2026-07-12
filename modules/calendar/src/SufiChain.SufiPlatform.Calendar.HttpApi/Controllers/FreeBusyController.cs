using Microsoft.AspNetCore.Mvc;
using SufiChain.SufiPlatform.Application.Dtos;
using SufiChain.SufiPlatform.Calendar.FreeBusy;
using Volo.Abp;

namespace SufiChain.SufiPlatform.Calendar.Controllers;

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
