using Microsoft.Extensions.DependencyInjection;
using SufiChain.Chat.ConversationLinks;
using SufiChain.Chat.EntityFrameworkCore;
using SufiChain.Chat.Messages;
using SufiChain.Chat.Participants;
using SufiChain.Chat.Sessions;
using SufiChain.Chat.Usage;
using SufiChain.SufiAbp.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace SufiChain.Chat;

[DependsOn(
    typeof(ChatDomainModule),
    typeof(SufiAbpEntityFrameworkCoreModule)
)]
public class ChatEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<ChatDbContext>(options =>
        {
            options.AddDefaultRepositories();
            options.AddRepository<ChatSession, EfCoreChatSessionRepository>();
            options.AddRepository<ChatMessage, EfCoreChatMessageRepository>();
            options.AddRepository<ChatParticipant, EfCoreChatParticipantRepository>();
            options.AddRepository<ConversationLink, EfCoreConversationLinkRepository>();
            options.AddRepository<ChatUsageCounter, EfCoreChatUsageCounterRepository>();
            options.AddRepository<ChatAiUsageReservation, EfCoreChatAiUsageReservationRepository>();
        });
    }
}
