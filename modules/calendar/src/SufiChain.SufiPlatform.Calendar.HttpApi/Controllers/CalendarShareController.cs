using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SufiChain.SufiPlatform.Application.Dtos;
using SufiChain.SufiPlatform.Calendar.Calendars;
using SufiChain.SufiPlatform.Calendar.Permissions;
using Volo.Abp;

namespace SufiChain.SufiPlatform.Calendar.Controllers;

[Authorize(CalendarPermissions.Calendars.Share)]
[Area(CalendarRemoteServiceConsts.ModuleName)]
[RemoteService(Name = CalendarRemoteServiceConsts.RemoteServiceName)]
[Route("api/calendar/calendars")]
public class CalendarShareController : CalendarController, ICalendarShareAppService
{
    private readonly ICalendarShareAppService _calendarShareAppService;

    public CalendarShareController(ICalendarShareAppService calendarShareAppService)
    {
        _calendarShareAppService = calendarShareAppService;
    }

    [HttpGet]
    [Route("{calendarId}/shares")]
    public virtual Task<ListResultDto<CalendarShareDto>> GetListAsync(Guid calendarId)
    {
        return _calendarShareAppService.GetListAsync(calendarId);
    }

    [HttpPost]
    [Route("{calendarId}/shares")]
    public virtual Task<CalendarShareDto> GrantAsync(Guid calendarId, GrantCalendarShareInput input)
    {
        return _calendarShareAppService.GrantAsync(calendarId, input);
    }

    [HttpDelete]
    [Route("{calendarId}/shares/{shareId}")]
    public virtual Task RevokeAsync(Guid calendarId, Guid shareId)
    {
        return _calendarShareAppService.RevokeAsync(calendarId, shareId);
    }
}
