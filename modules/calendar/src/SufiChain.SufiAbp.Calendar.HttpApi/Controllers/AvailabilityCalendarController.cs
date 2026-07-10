using Microsoft.AspNetCore.Mvc;
using SufiChain.SufiAbp.Application.Dtos;
using SufiChain.SufiAbp.Calendar.Availability;
using SufiChain.SufiAbp.Calendar.Calendars;
using Volo.Abp;

namespace SufiChain.SufiAbp.Calendar.Controllers;

[Area(CalendarRemoteServiceConsts.ModuleName)]
[RemoteService(Name = CalendarRemoteServiceConsts.RemoteServiceName)]
[Route("api/calendar/calendars")]
public class AvailabilityCalendarController : CalendarController, IAvailabilityCalendarAppService
{
    private readonly IAvailabilityCalendarAppService _availabilityCalendarAppService;

    public AvailabilityCalendarController(IAvailabilityCalendarAppService availabilityCalendarAppService)
    {
        _availabilityCalendarAppService = availabilityCalendarAppService;
    }

    [HttpGet]
    [Route("{id}")]
    public virtual Task<CalendarDto> GetAsync(Guid id)
    {
        return _availabilityCalendarAppService.GetAsync(id);
    }

    [HttpGet]
    public virtual Task<PagedResultDto<CalendarDto>> GetListAsync([FromQuery] GetCalendarListInput input)
    {
        return _availabilityCalendarAppService.GetListAsync(input);
    }

    [HttpGet]
    [Route("lookup")]
    public virtual Task<ListResultDto<CalendarLookupDto>> GetLookupAsync([FromQuery] CalendarKind? kind = null)
    {
        return _availabilityCalendarAppService.GetLookupAsync(kind);
    }

    [HttpGet]
    [Route("default/{kind}")]
    public virtual Task<CalendarDto?> GetDefaultAsync(CalendarKind kind)
    {
        return _availabilityCalendarAppService.GetDefaultAsync(kind);
    }

    [HttpGet]
    [Route("my/personal")]
    public virtual Task<CalendarDto> GetOrCreateMyPersonalCalendarAsync()
    {
        return _availabilityCalendarAppService.GetOrCreateMyPersonalCalendarAsync();
    }

    [HttpGet]
    [Route("my/visible")]
    public virtual Task<ListResultDto<CalendarLookupDto>> GetMyVisibleCalendarsAsync()
    {
        return _availabilityCalendarAppService.GetMyVisibleCalendarsAsync();
    }

    [HttpPost]
    [Route("organization-units")]
    public virtual Task<ListResultDto<CalendarLookupDto>> GetOrganizationUnitCalendarsAsync(List<Guid> organizationUnitIds)
    {
        return _availabilityCalendarAppService.GetOrganizationUnitCalendarsAsync(organizationUnitIds);
    }

    [HttpPost]
    public virtual Task<CalendarDto> CreateAsync(CreateUpdateCalendarDto input)
    {
        return _availabilityCalendarAppService.CreateAsync(input);
    }

    [HttpPut]
    [Route("{id}")]
    public virtual Task<CalendarDto> UpdateAsync(Guid id, CreateUpdateCalendarDto input)
    {
        return _availabilityCalendarAppService.UpdateAsync(id, input);
    }

    [HttpDelete]
    [Route("{id}")]
    public virtual Task DeleteAsync(Guid id)
    {
        return _availabilityCalendarAppService.DeleteAsync(id);
    }

    [HttpGet]
    [Route("{calendarId}/working-hours")]
    public virtual Task<ListResultDto<WorkingHourRuleDto>> GetWorkingHoursAsync(Guid calendarId)
    {
        return _availabilityCalendarAppService.GetWorkingHoursAsync(calendarId);
    }

    [HttpPut]
    [Route("{calendarId}/working-hours")]
    public virtual Task<ListResultDto<WorkingHourRuleDto>> ReplaceWorkingHoursAsync(Guid calendarId, List<CreateUpdateWorkingHourRuleDto> input)
    {
        return _availabilityCalendarAppService.ReplaceWorkingHoursAsync(calendarId, input);
    }

    [HttpGet]
    [Route("{calendarId}/exceptions")]
    public virtual Task<ListResultDto<CalendarExceptionDto>> GetExceptionsAsync(Guid calendarId)
    {
        return _availabilityCalendarAppService.GetExceptionsAsync(calendarId);
    }

    [HttpPut]
    [Route("{calendarId}/exceptions")]
    public virtual Task<ListResultDto<CalendarExceptionDto>> ReplaceExceptionsAsync(Guid calendarId, List<CreateUpdateCalendarExceptionDto> input)
    {
        return _availabilityCalendarAppService.ReplaceExceptionsAsync(calendarId, input);
    }

    [HttpGet]
    [Route("{calendarId}/inheritances")]
    public virtual Task<ListResultDto<CalendarInheritanceDto>> GetInheritancesAsync(Guid calendarId)
    {
        return _availabilityCalendarAppService.GetInheritancesAsync(calendarId);
    }

    [HttpPost]
    [Route("{calendarId}/inheritances")]
    public virtual Task<CalendarDto> AddInheritanceAsync(Guid calendarId, AddCalendarInheritanceInput input)
    {
        return _availabilityCalendarAppService.AddInheritanceAsync(calendarId, input);
    }

    [HttpPut]
    [Route("{calendarId}/inheritances/{parentCalendarId}")]
    public virtual Task<CalendarInheritanceDto> UpdateInheritanceAsync(Guid calendarId, Guid parentCalendarId, UpdateCalendarInheritanceInput input)
    {
        return _availabilityCalendarAppService.UpdateInheritanceAsync(calendarId, parentCalendarId, input);
    }

    [HttpDelete]
    [Route("{calendarId}/inheritances/{parentCalendarId}")]
    public virtual Task DeleteInheritanceAsync(Guid calendarId, Guid parentCalendarId)
    {
        return _availabilityCalendarAppService.DeleteInheritanceAsync(calendarId, parentCalendarId);
    }

    [HttpGet]
    [Route("{calendarId}/eligible-parents")]
    public virtual Task<ListResultDto<CalendarLookupDto>> GetEligibleParentCalendarsAsync(Guid calendarId)
    {
        return _availabilityCalendarAppService.GetEligibleParentCalendarsAsync(calendarId);
    }

    [HttpPost]
    [Route("{calendarId}/test")]
    public virtual Task<TestAvailabilityResultDto> TestAsync(Guid calendarId, TestAvailabilityInput input)
    {
        return _availabilityCalendarAppService.TestAsync(calendarId, input);
    }
}
