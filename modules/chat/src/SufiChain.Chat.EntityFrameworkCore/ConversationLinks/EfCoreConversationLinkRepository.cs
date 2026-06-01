using Microsoft.EntityFrameworkCore;
using SufiChain.Chat.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.Chat.ConversationLinks;

public class EfCoreConversationLinkRepository : EfCoreRepository<ChatDbContext, ConversationLink, Guid>, IConversationLinkRepository
{
    public EfCoreConversationLinkRepository(IDbContextProvider<ChatDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public virtual async Task<List<ConversationLink>> GetListBySessionAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        return await (await GetDbSetAsync())
            .Where(x => x.SessionId == sessionId)
            .ToListAsync(GetCancellationToken(cancellationToken));
    }

    public virtual async Task<List<ConversationLink>> GetListByEntityAsync(
        string linkedEntityType,
        string linkedEntityId,
        CancellationToken cancellationToken = default)
    {
        return await (await GetDbSetAsync())
            .Where(x => x.LinkedEntityType == linkedEntityType && x.LinkedEntityId == linkedEntityId)
            .ToListAsync(GetCancellationToken(cancellationToken));
    }
}
