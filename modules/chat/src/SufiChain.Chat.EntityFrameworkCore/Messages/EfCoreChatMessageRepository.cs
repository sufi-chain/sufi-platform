using Microsoft.EntityFrameworkCore;
using SufiChain.Chat.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.Chat.Messages;

public class EfCoreChatMessageRepository : EfCoreRepository<ChatDbContext, ChatMessage, Guid>, IChatMessageRepository
{
    public EfCoreChatMessageRepository(IDbContextProvider<ChatDbContext> dbContextProvider)
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
        return await (await GetDbSetAsync())
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
        return await (await GetDbSetAsync())
            .Where(x => x.SessionId == sessionId)
            .WhereIf(!includeInternal, x => !x.IsInternal)
            .LongCountAsync(GetCancellationToken(cancellationToken));
    }
}
