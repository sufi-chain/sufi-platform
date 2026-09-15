using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiPlatform.Authorization;
using SufiChain.SufiPlatform.SufiCom.Channels.Sms.Kavenegar;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.Modularity;

using Volo.Abp.Autofac;
using Volo.Abp.Testing;
namespace SufiChain.SufiPlatform.SufiCom;

[DependsOn(
    typeof(SufiComDomainModule),
    typeof(AbpTestBaseModule),
    typeof(AbpAutofacModule),
    typeof(SufiAuthorizationModule),
    typeof(SufiComChannelsSmsKavenegarModule)
)]
public class SufiComTestBaseModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAlwaysAllowAuthorization();
    }
}
