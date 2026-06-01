using MongoDB.Driver;
using SufiChain.Chat.ConversationLinks;
using SufiChain.Chat.Messages;
using SufiChain.Chat.Participants;
using SufiChain.Chat.Sessions;
using SufiChain.Chat.Usage;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;

namespace SufiChain.Chat.MongoDB;

[ConnectionStringName(ChatDbProperties.ConnectionStringName)]
public interface IChatMongoDbContext : IAbpMongoDbContext
{
    IMongoCollection<ChatSession> Sessions { get; }

    IMongoCollection<ChatMessage> Messages { get; }

    IMongoCollection<ChatParticipant> Participants { get; }

    IMongoCollection<ConversationLink> ConversationLinks { get; }

    IMongoCollection<ChatUsageCounter> UsageCounters { get; }

    IMongoCollection<ChatAiUsageReservation> AiUsageReservations { get; }
}
