using MongoDB.Driver;
using SufiChain.Chat.ConversationLinks;
using SufiChain.Chat.Messages;
using SufiChain.Chat.Participants;
using SufiChain.Chat.Sessions;
using SufiChain.Chat.Usage;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MongoDB;

namespace SufiChain.Chat.MongoDB;

public class ChatMongoDbIndexesInitializer : ITransientDependency
{
    protected IMongoDbContextProvider<ChatMongoDbContext> DbContextProvider { get; }

    public ChatMongoDbIndexesInitializer(IMongoDbContextProvider<ChatMongoDbContext> dbContextProvider)
    {
        DbContextProvider = dbContextProvider;
    }

    public virtual async Task CreateIndexesAsync(CancellationToken cancellationToken = default)
    {
        var dbContext = await DbContextProvider.GetDbContextAsync();

        await CreateSessionIndexesAsync(dbContext.ChatSessions, cancellationToken);
        await CreateMessageIndexesAsync(dbContext.Messages, cancellationToken);
        await CreateParticipantIndexesAsync(dbContext.Participants, cancellationToken);
        await CreateConversationLinkIndexesAsync(dbContext.ConversationLinks, cancellationToken);
        await CreateUsageCounterIndexesAsync(dbContext.UsageCounters, cancellationToken);
        await CreateAiUsageReservationIndexesAsync(dbContext.AiUsageReservations, cancellationToken);
    }

    protected virtual async Task CreateSessionIndexesAsync(
        IMongoCollection<ChatSession> collection,
        CancellationToken cancellationToken)
    {
        await collection.Indexes.CreateManyAsync(
            new[]
            {
                new CreateIndexModel<ChatSession>(
                    Builders<ChatSession>.IndexKeys
                        .Ascending(x => x.TenantId)
                        .Ascending(x => x.Status)
                        .Descending(x => x.LastMessageTime)),
                new CreateIndexModel<ChatSession>(
                    Builders<ChatSession>.IndexKeys
                        .Ascending(x => x.TenantId)
                        .Ascending(x => x.AccessMode)
                        .Descending(x => x.LastMessageTime)),
                new CreateIndexModel<ChatSession>(
                    Builders<ChatSession>.IndexKeys.Descending(x => x.LastMessageTime))
            },
            cancellationToken);
    }

    protected virtual async Task CreateMessageIndexesAsync(
        IMongoCollection<ChatMessage> collection,
        CancellationToken cancellationToken)
    {
        await collection.Indexes.CreateManyAsync(
            new[]
            {
                new CreateIndexModel<ChatMessage>(
                    Builders<ChatMessage>.IndexKeys
                        .Ascending(x => x.SessionId)
                        .Ascending(x => x.CreationTime)),
                new CreateIndexModel<ChatMessage>(
                    Builders<ChatMessage>.IndexKeys
                        .Ascending(x => x.TenantId)
                        .Ascending(x => x.SessionId)
                        .Ascending(x => x.CreationTime))
            },
            cancellationToken);
    }

    protected virtual async Task CreateParticipantIndexesAsync(
        IMongoCollection<ChatParticipant> collection,
        CancellationToken cancellationToken)
    {
        await collection.Indexes.CreateManyAsync(
            new[]
            {
                new CreateIndexModel<ChatParticipant>(
                    Builders<ChatParticipant>.IndexKeys
                        .Ascending(x => x.TenantId)
                        .Ascending(x => x.SessionId)
                        .Ascending(x => x.UserId)),
                new CreateIndexModel<ChatParticipant>(
                    Builders<ChatParticipant>.IndexKeys
                        .Ascending(x => x.TenantId)
                        .Ascending(x => x.SessionId)
                        .Ascending(x => x.AnonymousVisitorId)),
                new CreateIndexModel<ChatParticipant>(
                    Builders<ChatParticipant>.IndexKeys
                        .Ascending(x => x.TenantId)
                        .Ascending(x => x.UserId)
                        .Ascending(x => x.LeftAt))
            },
            cancellationToken);
    }

    protected virtual async Task CreateConversationLinkIndexesAsync(
        IMongoCollection<ConversationLink> collection,
        CancellationToken cancellationToken)
    {
        await collection.Indexes.CreateManyAsync(
            new[]
            {
                new CreateIndexModel<ConversationLink>(Builders<ConversationLink>.IndexKeys.Ascending(x => x.SessionId)),
                new CreateIndexModel<ConversationLink>(
                    Builders<ConversationLink>.IndexKeys
                        .Ascending(x => x.LinkedEntityType)
                        .Ascending(x => x.LinkedEntityId)),
                new CreateIndexModel<ConversationLink>(
                    Builders<ConversationLink>.IndexKeys
                        .Ascending(x => x.TenantId)
                        .Ascending(x => x.LinkedEntityType)
                        .Ascending(x => x.LinkedEntityId))
            },
            cancellationToken);
    }

    protected virtual async Task CreateUsageCounterIndexesAsync(
        IMongoCollection<ChatUsageCounter> collection,
        CancellationToken cancellationToken)
    {
        await collection.Indexes.CreateManyAsync(
            new[]
            {
                new CreateIndexModel<ChatUsageCounter>(
                    Builders<ChatUsageCounter>.IndexKeys
                        .Ascending(x => x.TenantId)
                        .Ascending(x => x.CounterKey)
                        .Ascending(x => x.Period)
                        .Ascending(x => x.PeriodStart),
                    new CreateIndexOptions { Unique = true }),
                new CreateIndexModel<ChatUsageCounter>(
                    Builders<ChatUsageCounter>.IndexKeys
                        .Ascending(x => x.TenantId)
                        .Ascending(x => x.PeriodStart))
            },
            cancellationToken);
    }

    protected virtual async Task CreateAiUsageReservationIndexesAsync(
        IMongoCollection<ChatAiUsageReservation> collection,
        CancellationToken cancellationToken)
    {
        await collection.Indexes.CreateManyAsync(
            new[]
            {
                new CreateIndexModel<ChatAiUsageReservation>(
                    Builders<ChatAiUsageReservation>.IndexKeys
                        .Ascending(x => x.TenantId)
                        .Ascending(x => x.ReservedAt)),
                new CreateIndexModel<ChatAiUsageReservation>(
                    Builders<ChatAiUsageReservation>.IndexKeys
                        .Ascending(x => x.TenantId)
                        .Ascending(x => x.SessionId)
                        .Ascending(x => x.OperationKind)
                        .Ascending(x => x.ReservedAt)),
                new CreateIndexModel<ChatAiUsageReservation>(
                    Builders<ChatAiUsageReservation>.IndexKeys
                        .Ascending(x => x.SessionId)
                        .Ascending(x => x.OperationKind)
                        .Ascending(x => x.Status)),
                new CreateIndexModel<ChatAiUsageReservation>(
                    Builders<ChatAiUsageReservation>.IndexKeys
                        .Ascending(x => x.OperatorUserId)
                        .Ascending(x => x.OperationKind)
                        .Ascending(x => x.ReservedAt)),
                new CreateIndexModel<ChatAiUsageReservation>(
                    Builders<ChatAiUsageReservation>.IndexKeys
                        .Ascending(x => x.TenantId)
                        .Ascending(x => x.OperationKind)
                        .Ascending(x => x.RecordedAt))
            },
            cancellationToken);
    }
}
