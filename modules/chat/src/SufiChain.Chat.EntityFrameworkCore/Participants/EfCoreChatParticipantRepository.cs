using Microsoft.EntityFrameworkCore;
using SufiChain.Chat.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.Chat.Participants;

public class EfCoreChatParticipantRepository : EfCoreRepository<ChatDbContext, ChatParticipant, Guid>, IChatParticipantRepository
{
    public EfCoreChatParticipantRepository(IDbContextProvider<ChatDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public virtual async Task<List<ChatParticipant>> GetListBySessionAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        return await (await GetDbSetAsync())
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
        return await (await GetDbSetAsync())
            .Where(x => x.SessionId == sessionId && x.LeftAt == null)
            .WhereIf(userId.HasValue, x => x.UserId == userId)
            .WhereIf(!anonymousVisitorId.IsNullOrWhiteSpace(), x => x.AnonymousVisitorId == anonymousVisitorId)
            .AnyAsync(GetCancellationToken(cancellationToken));
    }

    public virtual async Task<int> GetActiveCountAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        return await (await GetDbSetAsync())
            .Where(x => x.SessionId == sessionId && x.LeftAt == null)
            .CountAsync(GetCancellationToken(cancellationToken));
    }
}
