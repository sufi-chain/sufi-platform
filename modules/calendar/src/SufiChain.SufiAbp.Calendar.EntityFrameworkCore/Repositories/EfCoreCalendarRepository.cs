using Microsoft.EntityFrameworkCore;
using SufiChain.SufiAbp.Calendar.Calendars;
using SufiChain.SufiAbp.Calendar.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiAbp.Calendar.EntityFrameworkCore.Repositories;

public class EfCoreCalendarRepository : EfCoreRepository<ICalendarDbContext, Calendars.Calendar, Guid>, ICalendarRepository
{
    public EfCoreCalendarRepository(IDbContextProvider<ICalendarDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public virtual async Task<Calendars.Calendar?> FindDefaultAsync(Guid? tenantId, CalendarKind kind, CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Include(x => x.WorkingHourRules)
            .Include(x => x.Exceptions)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Kind == kind && x.IsDefault, cancellationToken);
    }

    public override async Task<IQueryable<Calendars.Calendar>> WithDetailsAsync()
    {
        return (await GetQueryableAsync())
            .Include(x => x.WorkingHourRules)
            .Include(x => x.Exceptions);
    }
}
