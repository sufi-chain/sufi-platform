using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.Data;
using Volo.Abp.Application;
using Volo.Abp.Mapperly;
using SufiChain.SufiAbp.Identity;
using SufiChain.SufiAbp.Identity.Integration;
using Volo.Abp.Modularity;
using SufiChain.SufiAbp.Ddd;
using SufiChain.SufiAbp.Mapperly;

namespace SufiChain.SufiAbp.Identity;

[DependsOn(
    typeof(SufiAbpDataModule),
    typeof(SufiAbpIdentityApplicationContractsModule),
    typeof(SufiAbpDddApplicationModule),
    typeof(SufiAbpMapperlyModule),
    typeof(SufiAbpIdentityDomainModule)
)]
public class SufiAbpIdentityApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMapperlyObjectMapper<SufiAbpIdentityApplicationModule>();
        context.Services.AddTransient<IIdentityRoleAppService, IdentityRoleAppService>();
        context.Services.AddTransient<IIdentityUserAppService, IdentityUserAppService>();
        context.Services.AddTransient<IIdentityUserIntegrationService, IdentityUserIntegrationService>();
    }
}
