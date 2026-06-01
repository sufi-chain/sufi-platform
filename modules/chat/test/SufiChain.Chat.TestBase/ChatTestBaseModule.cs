using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.Autofac;
using SufiChain.SufiAbp.Authorization;
using SufiChain.SufiAbp.TestBase;
using Volo.Abp.Authorization;
using Volo.Abp.Modularity;

namespace SufiChain.Chat;

[DependsOn(
    typeof(SufiAbpAutofacModule),
    typeof(SufiAbpTestBaseModule),
    typeof(SufiAbpAuthorizationModule),
    typeof(ChatDomainModule)
)]
public class ChatTestBaseModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAlwaysAllowAuthorization();
    }
}
