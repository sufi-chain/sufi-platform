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

    Task<CalendarDto> GetOrCreateMyPersonalCalendarAsync();

    Task<ListResultDto<CalendarLookupDto>> GetMyVisibleCalendarsAsync();

    Task<ListResultDto<CalendarLookupDto>> GetOrganizationUnitCalendarsAsync(List<Guid> organizationUnitIds);

    Task<CalendarDto> CreateAsync(CreateUpdateCalendarDto input);

    Task<CalendarDto> UpdateAsync(Guid id, CreateUpdateCalendarDto input);

    Task DeleteAsync(Guid id);

    Task<ListResultDto<WorkingHourRuleDto>> GetWorkingHoursAsync(Guid calendarId);

    Task<ListResultDto<WorkingHourRuleDto>> ReplaceWorkingHoursAsync(Guid calendarId, List<CreateUpdateWorkingHourRuleDto> input);

    Task<ListResultDto<CalendarExceptionDto>> GetExceptionsAsync(Guid calendarId);

    Task<ListResultDto<CalendarExceptionDto>> ReplaceExceptionsAsync(Guid calendarId, List<CreateUpdateCalendarExceptionDto> input);

    Task<ListResultDto<CalendarInheritanceDto>> GetInheritancesAsync(Guid calendarId);

    Task<CalendarDto> AddInheritanceAsync(Guid calendarId, AddCalendarInheritanceInput input);

    Task<CalendarInheritanceDto> UpdateInheritanceAsync(Guid calendarId, Guid parentCalendarId, UpdateCalendarInheritanceInput input);

    Task DeleteInheritanceAsync(Guid calendarId, Guid parentCalendarId);

    Task<ListResultDto<CalendarLookupDto>> GetEligibleParentCalendarsAsync(Guid calendarId);

    Task<TestAvailabilityResultDto> TestAsync(Guid calendarId, TestAvailabilityInput input);
}
