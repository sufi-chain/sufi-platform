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
            .Include(x => x.Inheritances)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Kind == kind && x.IsDefault, cancellationToken);
    }

    public virtual async Task<Calendars.Calendar?> FindByKindAsync(Guid? tenantId, CalendarKind kind, CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Include(x => x.WorkingHourRules)
            .Include(x => x.Exceptions)
            .Include(x => x.Inheritances)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Kind == kind, cancellationToken);
    }

   public virtual async Task<List<Calendars.Calendar>> GetInheritedCalendarsAsync(Guid calendarId, CancellationToken cancellationToken = default)
   {
       var dbSet = await GetDbSetAsync();
       var parentIds = await dbSet
           .Where(x => x.Id == calendarId)
           .SelectMany(x => x.Inheritances)
           .Select(x => x.ParentCalendarId)
           .ToListAsync(cancellationToken);

       if (parentIds.Count == 0)
       {
           return new List<Calendars.Calendar>();
       }

       return await dbSet
           .Include(x => x.WorkingHourRules)
           .Include(x => x.Exceptions)
           .Where(x => parentIds.Contains(x.Id))
           .ToListAsync(cancellationToken);
   }

    public virtual async Task<List<Guid>> GetInheritedCalendarIdsAsync(Guid calendarId, CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Where(x => x.Id == calendarId)
            .SelectMany(x => x.Inheritances)
            .Select(x => x.ParentCalendarId)
            .ToListAsync(cancellationToken);
    }

    public virtual async Task<List<Guid>> GetInheritingCalendarIdsAsync(Guid parentCalendarId, CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .SelectMany(x => x.Inheritances)
            .Where(x => x.ParentCalendarId == parentCalendarId)
            .Select(x => x.CalendarId)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public virtual async Task<List<Calendars.Calendar>> GetByOwnerUserIdAsync(Guid? tenantId, Guid ownerUserId, CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Include(x => x.WorkingHourRules)
            .Include(x => x.Exceptions)
            .Include(x => x.Inheritances)
            .Where(x => x.TenantId == tenantId && x.OwnerUserId == ownerUserId)
            .ToListAsync(cancellationToken);
    }

    public virtual async Task<Calendars.Calendar?> FindByNameAsync(Guid? tenantId, string name, CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Include(x => x.WorkingHourRules)
            .Include(x => x.Exceptions)
            .Include(x => x.Inheritances)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Name == name, cancellationToken);
    }

    public override async Task<IQueryable<Calendars.Calendar>> WithDetailsAsync()
    {
        return (await GetQueryableAsync())
            .Include(x => x.WorkingHourRules)
            .Include(x => x.Exceptions)
            .Include(x => x.Inheritances);
    }
}
