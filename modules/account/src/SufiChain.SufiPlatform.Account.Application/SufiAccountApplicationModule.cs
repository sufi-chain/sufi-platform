using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiPlatform.Captcha;
using SufiChain.SufiPlatform.Identity;
using SufiChain.SufiPlatform.SufiCom;
using SufiChain.SufiPlatform.TextTemplating.Scriban;
using Volo.Abp.Mapperly;
using Volo.Abp.Modularity;
using Volo.Abp.Settings;
using Volo.Abp.VirtualFileSystem;

using Volo.Abp.Caching;
namespace SufiChain.SufiPlatform.Account;

[DependsOn(
    typeof(SufiAccountApplicationContractsModule),
    typeof(SufiIdentityDomainModule),
    typeof(SufiComModule),
    typeof(SufiTextTemplatingScribanModule),
    typeof(SufiCaptchaModule),
    typeof(AbpMapperlyModule),
    typeof(AbpCachingModule),
    typeof(AbpSettingsModule)
)]
public class SufiAccountApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMapperlyObjectMapper<SufiAccountApplicationModule>();

        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SufiAccountApplicationModule>();
        });

        Configure<SufiAccountUrlOptions>(_ => { });
    }
}
