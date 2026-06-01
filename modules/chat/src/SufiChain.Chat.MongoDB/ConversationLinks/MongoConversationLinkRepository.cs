using MongoDB.Driver.Linq;
using SufiChain.Chat.MongoDB;
using Volo.Abp.Domain.Repositories.MongoDB;
using Volo.Abp.MongoDB;

namespace SufiChain.Chat.ConversationLinks;

public class MongoConversationLinkRepository : MongoDbRepository<ChatMongoDbContext, ConversationLink, Guid>, IConversationLinkRepository
{
    public MongoConversationLinkRepository(IMongoDbContextProvider<ChatMongoDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public virtual async Task<List<ConversationLink>> GetListBySessionAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        return await (await GetQueryableAsync())
            .Where(x => x.SessionId == sessionId)
            .ToListAsync(GetCancellationToken(cancellationToken));
    }

    public virtual async Task<List<ConversationLink>> GetListByEntityAsync(
        string linkedEntityType,
        string linkedEntityId,
        CancellationToken cancellationToken = default)
    {
        return await (await GetQueryableAsync())
            .Where(x => x.LinkedEntityType == linkedEntityType && x.LinkedEntityId == linkedEntityId)
            .ToListAsync(GetCancellationToken(cancellationToken));
    }
}
