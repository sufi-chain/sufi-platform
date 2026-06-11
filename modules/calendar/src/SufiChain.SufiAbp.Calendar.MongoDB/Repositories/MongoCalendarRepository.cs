using MongoDB.Driver.Linq;
using SufiChain.SufiAbp.Calendar.Calendars;
using SufiChain.SufiAbp.Calendar.MongoDB;
using Volo.Abp.Domain.Repositories.MongoDB;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiAbp.Calendar.MongoDB.Repositories;

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
}
