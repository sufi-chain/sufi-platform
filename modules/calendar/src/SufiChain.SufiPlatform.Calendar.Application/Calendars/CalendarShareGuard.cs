using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Linq;
using Volo.Abp.Users;

namespace SufiChain.SufiPlatform.Calendar.Calendars;

public class CalendarShareGuard : ITransientDependency
{
    private readonly IRepository<CalendarShare, Guid> _shareRepository;
    private readonly IAsyncQueryableExecuter _asyncExecuter;
    private readonly ICurrentUser _currentUser;

    public CalendarShareGuard(
        IRepository<CalendarShare, Guid> shareRepository,
        IAsyncQueryableExecuter asyncExecuter,
        ICurrentUser currentUser)
    {
        _shareRepository = shareRepository;
        _asyncExecuter = asyncExecuter;
        _currentUser = currentUser;
    }

    public virtual async Task<IReadOnlyList<Guid>> GetSharedCalendarIdsAsync()
    {
        if (!_currentUser.Id.HasValue)
        {
            return [];
        }

        var userId = _currentUser.Id.Value;
        var query = await _shareRepository.GetQueryableAsync();
        return await _asyncExecuter.ToListAsync(query.Where(x => x.UserId == userId).Select(x => x.CalendarId));
    }

    public virtual async Task EnsureCanEditAsync(Calendar calendar)
    {
        if (!_currentUser.Id.HasValue || calendar.OwnerUserId == _currentUser.Id)
        {
            return;
        }

        var userId = _currentUser.Id.Value;
        var query = await _shareRepository.GetQueryableAsync();
        var shared = await _asyncExecuter.AnyAsync(
            query.Where(x => x.CalendarId == calendar.Id && x.UserId == userId));
        if (shared)
        {
            throw new BusinessException(CalendarErrorCodes.CalendarShareReadOnly);
        }
    }
}
