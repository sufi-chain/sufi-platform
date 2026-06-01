using MongoDB.Driver.Linq;
using SufiChain.Chat.MongoDB;
using Volo.Abp.Domain.Repositories.MongoDB;
using Volo.Abp.MongoDB;

namespace SufiChain.Chat.Participants;

public class MongoChatParticipantRepository : MongoDbRepository<ChatMongoDbContext, ChatParticipant, Guid>, IChatParticipantRepository
{
    public MongoChatParticipantRepository(IMongoDbContextProvider<ChatMongoDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public virtual async Task<List<ChatParticipant>> GetListBySessionAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        return await (await GetQueryableAsync())
            .Where(x => x.SessionId == sessionId)
            .OrderBy(x => x.JoinedAt)
            .ToListAsync(GetCancellationToken(cancellationToken));
    }

    public virtual async Task<bool> IsParticipantAsync(
        Guid sessionId,
        Guid? userId = null,
        string? anonymousVisitorId = null,
        CancellationToken cancellationToken = default)
    {
        return await (await GetQueryableAsync())
            .Where(x => x.SessionId == sessionId && x.LeftAt == null)
            .WhereIf(userId.HasValue, x => x.UserId == userId)
            .WhereIf(!anonymousVisitorId.IsNullOrWhiteSpace(), x => x.AnonymousVisitorId == anonymousVisitorId)
            .AnyAsync(GetCancellationToken(cancellationToken));
    }

    public virtual async Task<int> GetActiveCountAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        return (int)await (await GetQueryableAsync())
            .Where(x => x.SessionId == sessionId && x.LeftAt == null)
            .LongCountAsync(GetCancellationToken(cancellationToken));
    }
}
