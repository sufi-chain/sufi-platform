using System.Threading.Tasks;
using SufiChain.SufiPlatform.Application.Dtos;
using Volo.Abp.Application.Services;

namespace SufiChain.SufiPlatform.Calendar.Calendars;

public interface ICalendarShareAppService : IApplicationService
{
    Task<ListResultDto<CalendarShareDto>> GetListAsync(Guid calendarId);

    Task<CalendarShareDto> GrantAsync(Guid calendarId, GrantCalendarShareInput input);

    Task RevokeAsync(Guid calendarId, Guid shareId);
}
