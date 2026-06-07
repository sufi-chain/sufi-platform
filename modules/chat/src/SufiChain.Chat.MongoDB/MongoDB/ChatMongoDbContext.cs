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
public class ChatMongoDbContext : AbpMongoDbContext, IChatMongoDbContext
{
    public IMongoCollection<ChatSession> ChatSessions => Collection<ChatSession>();

    public IMongoCollection<ChatMessage> Messages => Collection<ChatMessage>();

    public IMongoCollection<ChatParticipant> Participants => Collection<ChatParticipant>();

    public IMongoCollection<ConversationLink> ConversationLinks => Collection<ConversationLink>();

    public IMongoCollection<ChatUsageCounter> UsageCounters => Collection<ChatUsageCounter>();

    public IMongoCollection<ChatAiUsageReservation> AiUsageReservations => Collection<ChatAiUsageReservation>();

    protected override void CreateModel(IMongoModelBuilder modelBuilder)
    {
        base.CreateModel(modelBuilder);

        modelBuilder.ConfigureChat();
    }
}
