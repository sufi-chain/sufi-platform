using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SufiChain.SufiPlatform.Application.Dtos;
using SufiChain.SufiPlatform.Application.Services;
using SufiChain.SufiPlatform.Calendar.Permissions;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

namespace SufiChain.SufiPlatform.Calendar.Calendars;

[Authorize(CalendarPermissions.Calendars.Share)]
public class CalendarShareAppService : SufiApplicationService, ICalendarShareAppService
{
    private readonly IRepository<CalendarShare, Guid> _shareRepository;
    private readonly ICalendarRepository _calendarRepository;

    public CalendarShareAppService(
        IRepository<CalendarShare, Guid> shareRepository,
        ICalendarRepository calendarRepository)
    {
        _shareRepository = shareRepository;
        _calendarRepository = calendarRepository;
    }

    public virtual async Task<ListResultDto<CalendarShareDto>> GetListAsync(Guid calendarId)
    {
        await _calendarRepository.GetAsync(calendarId);
        var query = await _shareRepository.GetQueryableAsync();
        var shares = await AsyncExecuter.ToListAsync(query.Where(x => x.CalendarId == calendarId));
        return new ListResultDto<CalendarShareDto>(shares.Select(ToDto).ToList());
    }

    public virtual async Task<CalendarShareDto> GrantAsync(Guid calendarId, GrantCalendarShareInput input)
    {
        await _calendarRepository.GetAsync(calendarId);
        if (input.UserId == Guid.Empty)
        {
            throw new BusinessException(CalendarErrorCodes.UserRequired);
        }

        var query = await _shareRepository.GetQueryableAsync();
        var existing = await AsyncExecuter.FirstOrDefaultAsync(
            query.Where(x => x.CalendarId == calendarId && x.UserId == input.UserId));
        if (existing != null)
        {
            return ToDto(existing);
        }

        var share = new CalendarShare(GuidGenerator.Create(), calendarId, input.UserId);
        await _shareRepository.InsertAsync(share, autoSave: true);
        return ToDto(share);
    }

    public virtual async Task RevokeAsync(Guid calendarId, Guid shareId)
    {
        var share = await _shareRepository.GetAsync(shareId);
        if (share.CalendarId != calendarId)
        {
            throw new BusinessException(CalendarErrorCodes.CalendarNotAccessible);
        }

        await _shareRepository.DeleteAsync(share, autoSave: true);
    }

    private static CalendarShareDto ToDto(CalendarShare share) => new()
    {
        Id = share.Id,
        CalendarId = share.CalendarId,
        UserId = share.UserId
    };
}
