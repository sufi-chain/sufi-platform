using SufiChain.SufiPlatform.Application.Dtos;
using SufiChain.SufiPlatform.Application.Services;

namespace SufiChain.SufiPlatform.Calendar.Events;

public interface ICalendarEventAppService : IApplicationService
{
    Task<CalendarEventDto> GetAsync(Guid id);

    Task<PagedResultDto<CalendarEventDto>> GetListAsync(GetEventListInput input);

    Task<ListResultDto<CalendarEventDto>> GetEventsBySourceAsync(string sourceType, string sourceId);

    Task<CalendarEventDto> CreateAsync(CreateUpdateCalendarEventDto input);

    Task<CalendarEventDto> UpdateAsync(Guid id, CreateUpdateCalendarEventDto input);

    Task DeleteAsync(Guid id);

    Task<ListResultDto<EventOccurrenceDto>> GetOccurrencesAsync(Guid calendarId, GetOccurrencesInput input);

    Task<CalendarEventDto> SetRecurrenceAsync(Guid id, string recurrenceRule);

    Task<CalendarEventDto> ClearRecurrenceAsync(Guid id);

    Task<CalendarEventDto> MoveOccurrenceAsync(Guid id, MoveOccurrenceDto input);

    Task<CalendarEventDto> CancelOccurrenceAsync(Guid id, CancelOccurrenceDto input);

    Task<CalendarEventDto> AddAttendeeAsync(Guid id, CreateEventAttendeeDto input);

    Task<CalendarEventDto> RemoveAttendeeAsync(Guid id, Guid attendeeId);

    Task<CalendarEventDto> SetRsvpAsync(Guid id, Guid attendeeId, RsvpStatus rsvpStatus);

    Task<CalendarEventDto> AddReminderAsync(Guid id, CreateEventReminderDto input);

    Task<CalendarEventDto> RemoveReminderAsync(Guid id, Guid reminderId);
}
