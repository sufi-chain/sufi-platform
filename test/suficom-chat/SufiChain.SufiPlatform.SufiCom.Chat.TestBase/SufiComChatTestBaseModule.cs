using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SufiChain.SufiPlatform.Authorization;
using SufiChain.SufiPlatform.Licensing;
using SufiChain.SufiPlatform.SufiCom.Chat.Licensing;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.Modularity;

using Volo.Abp.Autofac;
using Volo.Abp.Testing;
namespace SufiChain.SufiPlatform.SufiCom.Chat;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(AbpTestBaseModule),
    typeof(SufiAuthorizationModule),
    typeof(SufiComChatDomainModule)
)]
public class SufiComChatTestBaseModule : AbpModule
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
