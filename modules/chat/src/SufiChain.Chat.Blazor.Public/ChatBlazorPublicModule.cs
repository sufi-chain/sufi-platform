using Microsoft.Extensions.DependencyInjection;
using SufiChain.Chat.Blazor.Public.Localization;
using SufiChain.Chat.Blazor.Public.Services;
using SufiChain.Chat.Localization;
using SufiChain.SufiAbp.FileManager.Blazor.Public;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace SufiChain.Chat.Blazor.Public;

[DependsOn(
    typeof(ChatApplicationContractsModule),
    typeof(SufiAbpFileManagerBlazorPublicModule))]
public class ChatBlazorPublicModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddScoped<ChatMessengerState>();
        context.Services.AddScoped<ChatComposerJsInterop>();
        context.Services.AddScoped<IChatHubClientService, ChatHubClientService>();
        context.Services.AddScoped<IChatHubConnectionAccessTokenProvider, NullChatHubConnectionAccessTokenProvider>();

        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<ChatBlazorPublicModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<ChatPublicResource>("en")
                .AddBaseTypes(typeof(ChatResource))
                .AddVirtualJson("/Localization/ChatPublic");
        });
    }
}
