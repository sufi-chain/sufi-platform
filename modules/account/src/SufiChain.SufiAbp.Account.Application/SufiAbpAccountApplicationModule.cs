using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.Caching;
using SufiChain.SufiAbp.Captcha;
using SufiChain.SufiAbp.Identity;
using SufiChain.SufiAbp.Mapperly;
using SufiChain.SufiAbp.Messaging;
using SufiChain.SufiAbp.TextTemplating.Scriban;
using Volo.Abp.Mapperly;
using Volo.Abp.Modularity;
using Volo.Abp.Settings;
using Volo.Abp.VirtualFileSystem;

namespace SufiChain.SufiAbp.Account;

[DependsOn(
    typeof(SufiAbpAccountApplicationContractsModule),
    typeof(SufiAbpIdentityDomainModule),
    typeof(SufiAbpMessagingModule),
    typeof(SufiAbpTextTemplatingScribanModule),
    typeof(SufiAbpCaptchaModule),
    typeof(SufiAbpMapperlyModule),
    typeof(SufiAbpCachingModule),
    typeof(AbpSettingsModule)
)]
public class SufiAbpAccountApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMapperlyObjectMapper<SufiAbpAccountApplicationModule>();

        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SufiAbpAccountApplicationModule>();
        });

        Configure<SufiAbpAccountUrlOptions>(_ => { });
    }
}
