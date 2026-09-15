using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiPlatform.Authorization;
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
}
