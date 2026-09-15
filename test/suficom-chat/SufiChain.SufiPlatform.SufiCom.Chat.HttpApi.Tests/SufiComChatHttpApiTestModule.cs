using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiPlatform.SufiCom.Chat.Controllers;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.SufiCom.Chat.HttpApi;

[DependsOn(typeof(SufiComChatHttpApiModule), typeof(SufiComChatApplicationTestModule))]
public class SufiComChatHttpApiTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddTransient<ChatSessionController>();
        context.Services.AddTransient<ChatMessageController>();
    }
}
