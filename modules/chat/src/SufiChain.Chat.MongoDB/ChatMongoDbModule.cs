using Microsoft.Extensions.DependencyInjection;
using SufiChain.Chat.ConversationLinks;
using SufiChain.Chat.Messages;
using SufiChain.Chat.MongoDB;
using SufiChain.Chat.Participants;
using SufiChain.Chat.Sessions;
using SufiChain.Chat.Usage;
using SufiChain.SufiAbp.MongoDB;
using Volo.Abp.Modularity;

namespace SufiChain.Chat;

[DependsOn(
    typeof(ChatDomainModule),
    typeof(SufiAbpMongoDbModule)
)]
public class ChatMongoDbModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMongoDbContext<ChatMongoDbContext>(options =>
        {
            options.AddDefaultRepositories();
            options.AddRepository<ChatSession, MongoChatSessionRepository>();
            options.AddRepository<ChatMessage, MongoChatMessageRepository>();
            options.AddRepository<ChatParticipant, MongoChatParticipantRepository>();
            options.AddRepository<ConversationLink, MongoConversationLinkRepository>();
            options.AddRepository<ChatUsageCounter, MongoChatUsageCounterRepository>();
            options.AddRepository<ChatAiUsageReservation, MongoChatAiUsageReservationRepository>();
        });
    }
}
