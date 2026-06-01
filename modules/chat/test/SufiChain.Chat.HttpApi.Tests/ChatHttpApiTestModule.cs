using Microsoft.Extensions.DependencyInjection;
using SufiChain.Chat.Controllers;
using Volo.Abp.Modularity;

namespace SufiChain.Chat.HttpApi;

[DependsOn(typeof(ChatHttpApiModule), typeof(ChatApplicationTestModule))]
public class ChatHttpApiTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddTransient<ChatSessionController>();
        context.Services.AddTransient<ChatMessageController>();
    }
}
