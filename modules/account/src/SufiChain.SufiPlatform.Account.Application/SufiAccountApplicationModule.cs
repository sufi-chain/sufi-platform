using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SufiChain.SufiPlatform.Captcha;
using SufiChain.SufiPlatform.UI.Abstractions.Account;
using SufiChain.SufiPlatform.Identity;
using SufiChain.SufiPlatform.SufiCom;
using SufiChain.SufiPlatform.TextTemplating.Scriban;
using Volo.Abp.Mapperly;
using Volo.Abp.Modularity;
using Volo.Abp.Settings;
using Volo.Abp.VirtualFileSystem;

using Volo.Abp.Caching;
using Volo.Abp.DistributedLocking;
namespace SufiChain.SufiPlatform.Account;

[DependsOn(
    typeof(SufiAccountApplicationContractsModule),
    typeof(SufiIdentityDomainModule),
    typeof(SufiComModule),
    typeof(SufiTextTemplatingScribanModule),
    typeof(SufiCaptchaModule),
    typeof(AbpMapperlyModule),
    typeof(AbpCachingModule),
    typeof(AbpDistributedLockingAbstractionsModule),
    typeof(AbpSettingsModule)
)]
public class SufiAccountApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.Replace(ServiceDescriptor.Singleton<IPhoneConfirmationSessionStore, PhoneConfirmationSessionStore>());

        context.Services.AddMapperlyObjectMapper<SufiAccountApplicationModule>();

        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SufiAccountApplicationModule>();
        });

        Configure<SufiAccountUrlOptions>(_ => { });
    }
}
