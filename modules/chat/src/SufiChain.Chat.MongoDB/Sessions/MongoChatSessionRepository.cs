using MongoDB.Driver;
using MongoDB.Driver.Linq;
using SufiChain.Chat.Connectors.Metadata;
using SufiChain.Chat.MongoDB;
using Volo.Abp.Domain.Repositories.MongoDB;
using Volo.Abp.MongoDB;

namespace SufiChain.Chat.Sessions;

public class MongoChatSessionRepository : MongoDbRepository<ChatMongoDbContext, ChatSession, Guid>, IChatSessionRepository
{
    public MongoChatSessionRepository(IMongoDbContextProvider<ChatMongoDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public virtual async Task<ChatSession?> FindDirectSessionByUserPairAsync(
        Guid? tenantId,
        Guid userId,
        Guid otherUserId,
        CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();

        var sessionIds = await dbContext.Participants.AsQueryable()
            .Where(x => x.TenantId == tenantId && x.UserId == userId && x.LeftAt == null)
            .Select(x => x.SessionId)
            .ToListAsync(GetCancellationToken(cancellationToken));

        if (sessionIds.Count == 0)
        {
            return null;
        }

        var otherSessionIds = await dbContext.Participants.AsQueryable()
            .Where(x => x.TenantId == tenantId && x.UserId == otherUserId && x.LeftAt == null && sessionIds.Contains(x.SessionId))
            .Select(x => x.SessionId)
            .ToListAsync(GetCancellationToken(cancellationToken));

        if (otherSessionIds.Count == 0)
        {
            return null;
        }

        return await (await GetQueryableAsync())
            .Where(x => x.TenantId == tenantId && x.ConversationKind == ConversationKind.Direct && otherSessionIds.Contains(x.Id))
            .OrderByDescending(x => x.LastMessageTime ?? x.CreationTime)
            .FirstOrDefaultAsync(GetCancellationToken(cancellationToken));
    }

    public virtual async Task<List<ChatSession>> GetSessionsForParticipantAsync(
        Guid? tenantId,
        Guid? userId = null,
        string? anonymousVisitorId = null,
        int skipCount = 0,
        int maxResultCount = 10,
        CancellationToken cancellationToken = default)
    {
        var dbContext = await GetDbContextAsync();
        var participantQuery = dbContext.Participants.AsQueryable()
            .Where(x => x.TenantId == tenantId && x.LeftAt == null);

        if (userId.HasValue)
        {
            participantQuery = participantQuery.Where(x => x.UserId == userId);
        }

        if (!anonymousVisitorId.IsNullOrWhiteSpace())
        {
            participantQuery = participantQuery.Where(x => x.AnonymousVisitorId == anonymousVisitorId);
        }

        var sessionIds = await participantQuery
            .Select(x => x.SessionId)
            .ToListAsync(GetCancellationToken(cancellationToken));

        if (sessionIds.Count == 0)
        {
            return new List<ChatSession>();
        }

        return await (await GetQueryableAsync())
            .Where(x => x.TenantId == tenantId && sessionIds.Contains(x.Id))
            .OrderByDescending(x => x.LastMessageTime ?? x.CreationTime)
            .Skip(skipCount)
            .Take(maxResultCount)
            .ToListAsync(GetCancellationToken(cancellationToken));
    }

    public virtual async Task<ChatSession?> FindByConnectorExternalThreadAsync(
        Guid? tenantId,
        string connectorName,
        string externalThreadId,
        CancellationToken cancellationToken = default)
    {
        var lookupToken = ChatSessionConnectorMetadataMapper.BuildLookupToken(connectorName, externalThreadId);

        return await (await GetQueryableAsync())
            .Where(x => x.TenantId == tenantId && x.MetadataJson != null && x.MetadataJson.Contains(lookupToken))
            .OrderByDescending(x => x.LastMessageTime ?? x.CreationTime)
            .FirstOrDefaultAsync(GetCancellationToken(cancellationToken));
    }
}
