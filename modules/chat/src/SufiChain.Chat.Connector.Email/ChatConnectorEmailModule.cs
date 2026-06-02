using Microsoft.Extensions.DependencyInjection;
using SufiChain.Chat.Connectors.Email.Templates;
using SufiChain.SufiAbp.BackgroundWorkers;
using SufiChain.SufiAbp.Messaging;
using SufiChain.SufiAbp.TextTemplating;
using SufiChain.SufiAbp.TextTemplating.Scriban;
using Volo.Abp;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace SufiChain.Chat;

[DependsOn(
    typeof(ChatApplicationModule),
    typeof(ChatConnectorModule),
    typeof(SufiAbpMessagingModule),
    typeof(SufiAbpTextTemplatingModule),
    typeof(SufiAbpTextTemplatingScribanModule),
    typeof(SufiAbpBackgroundWorkersModule)
)]
public class ChatConnectorEmailModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<ChatConnectorEmailModule>();
        });
    }

    public override async Task OnApplicationInitializationAsync(ApplicationInitializationContext context)
    {
        await context.AddBackgroundWorkerAsync<Connectors.Email.ChatInboundEmailWorker>();
    }
}
