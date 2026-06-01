using SufiChain.Chat.ConversationLinks;
using SufiChain.Chat.Messages;
using SufiChain.Chat.Participants;
using SufiChain.Chat.Sessions;
using SufiChain.Chat.Usage;
using Volo.Abp;
using Volo.Abp.MongoDB;

namespace SufiChain.Chat.MongoDB;

public static class ChatMongoDbContextExtensions
{
    public static void ConfigureChat(this IMongoModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        builder.Entity<ChatSession>(b =>
        {
            b.CollectionName = ChatDbProperties.DbTablePrefix + "Sessions";
        });

        builder.Entity<ChatMessage>(b =>
        {
            b.CollectionName = ChatDbProperties.DbTablePrefix + "Messages";
        });

        builder.Entity<ChatParticipant>(b =>
        {
            b.CollectionName = ChatDbProperties.DbTablePrefix + "Participants";
        });

        builder.Entity<ConversationLink>(b =>
        {
            b.CollectionName = ChatDbProperties.DbTablePrefix + "ConversationLinks";
        });

        builder.Entity<ChatUsageCounter>(b =>
        {
            b.CollectionName = ChatDbProperties.DbTablePrefix + "UsageDailyAggregates";
        });

        builder.Entity<ChatAiUsageReservation>(b =>
        {
            b.CollectionName = ChatDbProperties.DbTablePrefix + "AiUsageReservations";
        });
    }
}
