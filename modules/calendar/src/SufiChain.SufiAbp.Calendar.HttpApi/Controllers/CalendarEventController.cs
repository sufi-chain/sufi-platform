using Microsoft.AspNetCore.Mvc;
using SufiChain.SufiAbp.Application.Dtos;
using SufiChain.SufiAbp.Calendar.Events;
using Volo.Abp;

namespace SufiChain.SufiAbp.Calendar.Controllers;

[Area(CalendarRemoteServiceConsts.ModuleName)]
[RemoteService(Name = CalendarRemoteServiceConsts.RemoteServiceName)]
[Route("api/calendar/events")]
public class CalendarEventController : CalendarController, ICalendarEventAppService
{
    private readonly ICalendarEventAppService _calendarEventAppService;

    public CalendarEventController(ICalendarEventAppService calendarEventAppService)
    {
        _calendarEventAppService = calendarEventAppService;
    }

    [HttpGet]
    [Route("{id}")]
    public virtual Task<CalendarEventDto> GetAsync(Guid id) => _calendarEventAppService.GetAsync(id);

    [HttpGet]
    public virtual Task<PagedResultDto<CalendarEventDto>> GetListAsync([FromQuery] GetEventListInput input) => _calendarEventAppService.GetListAsync(input);

    [HttpGet]
    [Route("by-source")]
    public virtual Task<ListResultDto<CalendarEventDto>> GetEventsBySourceAsync([FromQuery] string sourceType, [FromQuery] string sourceId) => _calendarEventAppService.GetEventsBySourceAsync(sourceType, sourceId);

    [HttpPost]
    public virtual Task<CalendarEventDto> CreateAsync(CreateUpdateCalendarEventDto input) => _calendarEventAppService.CreateAsync(input);

    [HttpPut]
    [Route("{id}")]
    public virtual Task<CalendarEventDto> UpdateAsync(Guid id, CreateUpdateCalendarEventDto input) => _calendarEventAppService.UpdateAsync(id, input);

    [HttpDelete]
    [Route("{id}")]
    public virtual Task DeleteAsync(Guid id) => _calendarEventAppService.DeleteAsync(id);

    [HttpPost]
    [Route("calendars/{calendarId}/occurrences")]
    public virtual Task<ListResultDto<EventOccurrenceDto>> GetOccurrencesAsync(Guid calendarId, GetOccurrencesInput input) => _calendarEventAppService.GetOccurrencesAsync(calendarId, input);

    [HttpPut]
    [Route("{id}/recurrence")]
    public virtual Task<CalendarEventDto> SetRecurrenceAsync(Guid id, [FromBody] string recurrenceRule) => _calendarEventAppService.SetRecurrenceAsync(id, recurrenceRule);

    [HttpDelete]
    [Route("{id}/recurrence")]
    public virtual Task<CalendarEventDto> ClearRecurrenceAsync(Guid id) => _calendarEventAppService.ClearRecurrenceAsync(id);

    [HttpPost]
    [Route("{id}/occurrences/move")]
    public virtual Task<CalendarEventDto> MoveOccurrenceAsync(Guid id, MoveOccurrenceDto input) => _calendarEventAppService.MoveOccurrenceAsync(id, input);

    [HttpPost]
    [Route("{id}/occurrences/cancel")]
    public virtual Task<CalendarEventDto> CancelOccurrenceAsync(Guid id, CancelOccurrenceDto input) => _calendarEventAppService.CancelOccurrenceAsync(id, input);

    [HttpPost]
    [Route("{id}/attendees")]
    public virtual Task<CalendarEventDto> AddAttendeeAsync(Guid id, CreateEventAttendeeDto input) => _calendarEventAppService.AddAttendeeAsync(id, input);

    [HttpDelete]
    [Route("{id}/attendees/{attendeeId}")]
    public virtual Task<CalendarEventDto> RemoveAttendeeAsync(Guid id, Guid attendeeId) => _calendarEventAppService.RemoveAttendeeAsync(id, attendeeId);

    [HttpPut]
    [Route("{id}/attendees/{attendeeId}/rsvp")]
    public virtual Task<CalendarEventDto> SetRsvpAsync(Guid id, Guid attendeeId, [FromBody] RsvpStatus rsvpStatus) => _calendarEventAppService.SetRsvpAsync(id, attendeeId, rsvpStatus);

    [HttpPost]
    [Route("{id}/reminders")]
    public virtual Task<CalendarEventDto> AddReminderAsync(Guid id, CreateEventReminderDto input) => _calendarEventAppService.AddReminderAsync(id, input);

    [HttpDelete]
    [Route("{id}/reminders/{reminderId}")]
    public virtual Task<CalendarEventDto> RemoveReminderAsync(Guid id, Guid reminderId) => _calendarEventAppService.RemoveReminderAsync(id, reminderId);
}
