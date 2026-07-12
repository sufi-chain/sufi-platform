using MongoDB.Driver.Linq;
using SufiChain.SufiPlatform.Calendar.Calendars;
using SufiChain.SufiPlatform.Calendar.MongoDB;
using Volo.Abp.Domain.Repositories.MongoDB;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiPlatform.Calendar.MongoDB.Repositories;

public class MongoCalendarRepository : MongoDbRepository<ICalendarMongoDbContext, Calendars.Calendar, Guid>, ICalendarRepository
{
    public MongoCalendarRepository(IMongoDbContextProvider<ICalendarMongoDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public virtual async Task<Calendars.Calendar?> FindDefaultAsync(Guid? tenantId, CalendarKind kind, CancellationToken cancellationToken = default)
    {
        return await (await GetQueryableAsync(cancellationToken))
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Kind == kind && x.IsDefault, cancellationToken);
    }

    public virtual async Task<Calendars.Calendar?> FindByKindAsync(Guid? tenantId, CalendarKind kind, CancellationToken cancellationToken = default)
    {
        return await (await GetQueryableAsync(cancellationToken))
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Kind == kind, cancellationToken);
    }

   public virtual async Task<List<Calendars.Calendar>> GetInheritedCalendarsAsync(Guid calendarId, CancellationToken cancellationToken = default)
   {
       var calendar = await (await GetQueryableAsync(cancellationToken))
           .FirstOrDefaultAsync(x => x.Id == calendarId, cancellationToken);

       if (calendar == null || calendar.Inheritances.Count == 0)
       {
           return new List<Calendars.Calendar>();
       }

       var parentIds = calendar.Inheritances.Select(x => x.ParentCalendarId).ToList();
       return await (await GetQueryableAsync(cancellationToken))
           .Where(x => parentIds.Contains(x.Id))
           .ToListAsync(cancellationToken);
   }

    public virtual async Task<List<Guid>> GetInheritedCalendarIdsAsync(Guid calendarId, CancellationToken cancellationToken = default)
    {
        var calendar = await (await GetQueryableAsync(cancellationToken))
            .FirstOrDefaultAsync(x => x.Id == calendarId, cancellationToken);

        return calendar == null || calendar.Inheritances.Count == 0
            ? new List<Guid>()
            : calendar.Inheritances.Select(x => x.ParentCalendarId).ToList();
    }

    public virtual async Task<List<Guid>> GetInheritingCalendarIdsAsync(Guid parentCalendarId, CancellationToken cancellationToken = default)
    {
        return await (await GetQueryableAsync(cancellationToken))
            .Where(x => x.Inheritances.Any(i => i.ParentCalendarId == parentCalendarId))
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);
    }

    public virtual async Task<List<Calendars.Calendar>> GetByOwnerUserIdAsync(Guid? tenantId, Guid ownerUserId, CancellationToken cancellationToken = default)
    {
        return await (await GetQueryableAsync(cancellationToken))
            .Where(x => x.TenantId == tenantId && x.OwnerUserId == ownerUserId)
            .ToListAsync(cancellationToken);
    }

    public virtual async Task<Calendars.Calendar?> FindByNameAsync(Guid? tenantId, string name, CancellationToken cancellationToken = default)
    {
        return await (await GetQueryableAsync(cancellationToken))
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Name == name, cancellationToken);
    }
}
