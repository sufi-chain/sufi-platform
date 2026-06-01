using Microsoft.EntityFrameworkCore;
using SufiChain.Chat.Connectors.Metadata;
using SufiChain.Chat.EntityFrameworkCore;
using SufiChain.Chat.Participants;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.Chat.Sessions;

public class EfCoreChatSessionRepository : EfCoreRepository<ChatDbContext, ChatSession, Guid>, IChatSessionRepository
{
    public EfCoreChatSessionRepository(IDbContextProvider<ChatDbContext> dbContextProvider)
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

        var sessionIds = await dbContext.Participants
            .Where(x => x.TenantId == tenantId && x.UserId == userId && x.LeftAt == null)
            .Select(x => x.SessionId)
            .ToListAsync(GetCancellationToken(cancellationToken));

        if (sessionIds.Count == 0)
        {
            return null;
        }

        var otherSessionIds = await dbContext.Participants
            .Where(x => x.TenantId == tenantId && x.UserId == otherUserId && x.LeftAt == null && sessionIds.Contains(x.SessionId))
            .Select(x => x.SessionId)
            .ToListAsync(GetCancellationToken(cancellationToken));

        if (otherSessionIds.Count == 0)
        {
            return null;
        }

        return await (await GetDbSetAsync())
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
        var sessionIdsQuery = dbContext.Participants
            .Where(x => x.TenantId == tenantId && x.LeftAt == null)
            .WhereIf(userId.HasValue, x => x.UserId == userId)
            .WhereIf(!anonymousVisitorId.IsNullOrWhiteSpace(), x => x.AnonymousVisitorId == anonymousVisitorId)
            .Select(x => x.SessionId);

        return await (await GetDbSetAsync())
            .Where(x => x.TenantId == tenantId && sessionIdsQuery.Contains(x.Id))
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

        return await (await GetDbSetAsync())
            .Where(x => x.TenantId == tenantId && x.MetadataJson != null && x.MetadataJson.Contains(lookupToken))
            .OrderByDescending(x => x.LastMessageTime ?? x.CreationTime)
            .FirstOrDefaultAsync(GetCancellationToken(cancellationToken));
    }
}
