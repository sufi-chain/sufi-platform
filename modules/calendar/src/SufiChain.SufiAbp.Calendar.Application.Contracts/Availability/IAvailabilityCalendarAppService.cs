using SufiChain.SufiAbp.Application.Dtos;
using SufiChain.SufiAbp.Application.Services;
using SufiChain.SufiAbp.Calendar.Calendars;

namespace SufiChain.SufiAbp.Calendar.Availability;

public interface IAvailabilityCalendarAppService : IApplicationService
{
    Task<CalendarDto> GetAsync(Guid id);

    Task<PagedResultDto<CalendarDto>> GetListAsync(GetCalendarListInput input);

    Task<ListResultDto<CalendarLookupDto>> GetLookupAsync(CalendarKind? kind = null);

    Task<CalendarDto?> GetDefaultAsync(CalendarKind kind);

    Task<CalendarDto> CreateAsync(CreateUpdateCalendarDto input);

    Task<CalendarDto> UpdateAsync(Guid id, CreateUpdateCalendarDto input);

    Task DeleteAsync(Guid id);

    Task<ListResultDto<WorkingHourRuleDto>> GetWorkingHoursAsync(Guid calendarId);

    Task<ListResultDto<WorkingHourRuleDto>> ReplaceWorkingHoursAsync(Guid calendarId, List<CreateUpdateWorkingHourRuleDto> input);

    Task<ListResultDto<CalendarExceptionDto>> GetExceptionsAsync(Guid calendarId);

    Task<ListResultDto<CalendarExceptionDto>> ReplaceExceptionsAsync(Guid calendarId, List<CreateUpdateCalendarExceptionDto> input);

    Task<TestAvailabilityResultDto> TestAsync(Guid calendarId, TestAvailabilityInput input);
}
