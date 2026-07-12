using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.Captcha;
using SufiChain.SufiAbp.Identity;
using SufiChain.SufiAbp.Communications;
using SufiChain.SufiAbp.TextTemplating.Scriban;
using Volo.Abp.Mapperly;
using Volo.Abp.Modularity;
using Volo.Abp.Settings;
using Volo.Abp.VirtualFileSystem;

using Volo.Abp.Caching;
namespace SufiChain.SufiAbp.Account;

[DependsOn(
    typeof(SufiAbpAccountApplicationContractsModule),
    typeof(SufiAbpIdentityDomainModule),
    typeof(SufiAbpCommunicationsModule),
    typeof(SufiAbpTextTemplatingScribanModule),
    typeof(SufiAbpCaptchaModule),
    typeof(AbpMapperlyModule),
    typeof(AbpCachingModule),
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
