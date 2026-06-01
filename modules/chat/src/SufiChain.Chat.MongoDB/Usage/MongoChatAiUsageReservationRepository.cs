using MongoDB.Driver.Linq;
using SufiChain.Chat.MongoDB;
using Volo.Abp.Domain.Repositories.MongoDB;
using Volo.Abp.MongoDB;

namespace SufiChain.Chat.Usage;

public class MongoChatAiUsageReservationRepository : MongoDbRepository<ChatMongoDbContext, ChatAiUsageReservation, Guid>, IChatAiUsageReservationRepository
{
    public MongoChatAiUsageReservationRepository(IMongoDbContextProvider<ChatMongoDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public virtual async Task<long> GetSessionAiReplyCountAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        return await (await GetQueryableAsync())
            .Where(x => x.SessionId == sessionId && x.OperationKind == ChatAiOperationKind.AutoReply && x.Status == ChatAiUsageReservationStatus.Recorded)
            .LongCountAsync(GetCancellationToken(cancellationToken));
    }

    public virtual async Task<long> GetSessionTokenCountAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        return await (await GetQueryableAsync())
            .Where(x => x.SessionId == sessionId && x.Status == ChatAiUsageReservationStatus.Recorded)
            .SumAsync(x => (long)(x.TotalTokens ?? 0), GetCancellationToken(cancellationToken));
    }

    public virtual async Task<long> GetTenantTokenCountAsync(
        Guid? tenantId,
        DateTime dayStart,
        CancellationToken cancellationToken = default)
    {
        var dayEnd = dayStart.AddDays(1);

        return await (await GetQueryableAsync())
            .Where(x => x.TenantId == tenantId && x.Status == ChatAiUsageReservationStatus.Recorded && x.RecordedAt >= dayStart && x.RecordedAt < dayEnd)
            .SumAsync(x => (long)(x.TotalTokens ?? 0), GetCancellationToken(cancellationToken));
    }

    public virtual async Task<long> GetOperatorOperationCountAsync(
        Guid operatorUserId,
        ChatAiOperationKind operationKind,
        DateTime dayStart,
        CancellationToken cancellationToken = default)
    {
        var dayEnd = dayStart.AddDays(1);

        return await (await GetQueryableAsync())
            .Where(x => x.OperatorUserId == operatorUserId && x.OperationKind == operationKind && x.Status == ChatAiUsageReservationStatus.Recorded && x.RecordedAt >= dayStart && x.RecordedAt < dayEnd)
            .LongCountAsync(GetCancellationToken(cancellationToken));
    }
}
