using Microsoft.AspNetCore.Mvc;
using SufiChain.SufiPlatform.Calendar.Calendars;
using Volo.Abp;

namespace SufiChain.SufiPlatform.Calendar.Controllers;

[Area(CalendarRemoteServiceConsts.ModuleName)]
[RemoteService(Name = CalendarRemoteServiceConsts.RemoteServiceName)]
[Route("api/calendar/integration/calendars")]
public class CalendarCatalogIntegrationController :
    CalendarController,
    ICalendarCatalogIntegrationService
{
    private readonly ICalendarCatalogIntegrationService _service;

    public CalendarCatalogIntegrationController(ICalendarCatalogIntegrationService service)
    {
        _service = service;
    }

    [HttpGet]
    public virtual Task<List<CalendarLookupDto>> GetVisibleCalendarsAsync([FromQuery] string? filter = null)
    {
        return _service.GetVisibleCalendarsAsync(filter);
    }
}
