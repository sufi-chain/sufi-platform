using SufiChain.Chat.Localization;
using SufiChain.SufiAbp.UI.Localization;
using SufiChain.SufiAbp.UI;
using Volo.Abp.Localization;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace SufiChain.Chat;

[DependsOn(typeof(SufiAbpUiDomainSharedModule))]
public class ChatDomainSharedModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<ChatDomainSharedModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<ChatResource>("en")
                .AddBaseTypes(typeof(SufiAbpFrameworkResource))
                .AddVirtualJson("/Localization/Chat");
        });

        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            options.MapCodeNamespace("Chat", typeof(ChatResource));
        });
    }
}
