using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SufiChain.SufiPlatform.Authorization;
using SufiChain.SufiPlatform.Licensing;
using SufiChain.SufiPlatform.SufiCom.Channels.Sms.Kavenegar;
using SufiChain.SufiPlatform.SufiCom.Licensing;
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

    public override void PostConfigureServices(ServiceConfigurationContext context)
    {
        // Runs after SufiLicensingRuntimeModule registers the fail-closed LicenseGate.
        context.Services.Replace(ServiceDescriptor.Singleton<ILicenseGate, AllowAllLicenseGate>());
    }
}
