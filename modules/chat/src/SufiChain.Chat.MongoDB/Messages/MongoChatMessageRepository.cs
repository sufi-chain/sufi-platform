using MongoDB.Driver.Linq;
using SufiChain.Chat.MongoDB;
using Volo.Abp.Domain.Repositories.MongoDB;
using Volo.Abp.MongoDB;

namespace SufiChain.Chat.Messages;

public class MongoChatMessageRepository : MongoDbRepository<ChatMongoDbContext, ChatMessage, Guid>, IChatMessageRepository
{
    public MongoChatMessageRepository(IMongoDbContextProvider<ChatMongoDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public virtual async Task<List<ChatMessage>> GetListBySessionAsync(
        Guid sessionId,
        bool includeInternal = false,
        int skipCount = 0,
        int maxResultCount = 50,
        CancellationToken cancellationToken = default)
    {
        return await (await GetQueryableAsync())
            .Where(x => x.SessionId == sessionId)
            .WhereIf(!includeInternal, x => !x.IsInternal)
            .OrderBy(x => x.CreationTime)
            .Skip(skipCount)
            .Take(maxResultCount)
            .ToListAsync(GetCancellationToken(cancellationToken));
    }

    public virtual async Task<long> GetCountBySessionAsync(
        Guid sessionId,
        bool includeInternal = false,
        CancellationToken cancellationToken = default)
    {
        return await (await GetQueryableAsync())
            .Where(x => x.SessionId == sessionId)
            .WhereIf(!includeInternal, x => !x.IsInternal)
            .LongCountAsync(GetCancellationToken(cancellationToken));
    }
}
