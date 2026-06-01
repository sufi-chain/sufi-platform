using MongoDB.Driver.Linq;
using SufiChain.Chat.MongoDB;
using Volo.Abp.Domain.Repositories.MongoDB;
using Volo.Abp.Guids;
using Volo.Abp.MongoDB;

namespace SufiChain.Chat.Usage;

public class MongoChatUsageCounterRepository : MongoDbRepository<ChatMongoDbContext, ChatUsageCounter, Guid>, IChatUsageCounterRepository
{
    protected new IGuidGenerator GuidGenerator { get; }

    public MongoChatUsageCounterRepository(
        IMongoDbContextProvider<ChatMongoDbContext> dbContextProvider,
        IGuidGenerator guidGenerator)
        : base(dbContextProvider)
    {
        GuidGenerator = guidGenerator;
    }

    public virtual async Task<long> GetCountAsync(
        Guid? tenantId,
        string counterKey,
        ChatUsageCounterPeriod period,
        DateTime periodStart,
        CancellationToken cancellationToken = default)
    {
        return await (await GetQueryableAsync())
            .Where(x => x.TenantId == tenantId && x.CounterKey == counterKey && x.Period == period && x.PeriodStart == periodStart)
            .Select(x => x.Count)
            .FirstOrDefaultAsync(GetCancellationToken(cancellationToken));
    }

    public virtual async Task IncrementAsync(
        Guid? tenantId,
        string counterKey,
        ChatUsageCounterPeriod period,
        DateTime periodStart,
        DateTime periodEnd,
        long count = 1,
        long tokenCount = 0,
        CancellationToken cancellationToken = default)
    {
        var counter = await (await GetQueryableAsync())
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId && x.CounterKey == counterKey && x.Period == period && x.PeriodStart == periodStart,
                GetCancellationToken(cancellationToken));

        if (counter == null)
        {
            counter = new ChatUsageCounter(GuidGenerator.Create(), tenantId, counterKey, period, periodStart, periodEnd);
            counter.Increment(count, tokenCount);
            await InsertAsync(counter, autoSave: true, cancellationToken: cancellationToken);
            return;
        }

        counter.Increment(count, tokenCount);
        await UpdateAsync(counter, autoSave: true, cancellationToken: cancellationToken);
    }
}
