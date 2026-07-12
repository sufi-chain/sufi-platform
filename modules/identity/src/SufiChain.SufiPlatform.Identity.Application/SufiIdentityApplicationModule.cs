using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiPlatform.Data;
using Volo.Abp.Application;
using Volo.Abp.Mapperly;
using SufiChain.SufiPlatform.Identity;
using SufiChain.SufiPlatform.Identity.Integration;
using Volo.Abp.Modularity;
using SufiChain.SufiPlatform.Ddd;

namespace SufiChain.SufiPlatform.Identity;

[DependsOn(
    typeof(SufiDataModule),
    typeof(SufiIdentityApplicationContractsModule),
    typeof(SufiDddApplicationModule),
    typeof(AbpMapperlyModule),
    typeof(SufiIdentityDomainModule)
)]
public class SufiIdentityApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMapperlyObjectMapper<SufiIdentityApplicationModule>();
        context.Services.AddTransient<IIdentityRoleAppService, IdentityRoleAppService>();
        context.Services.AddTransient<IIdentityUserAppService, IdentityUserAppService>();
        context.Services.AddTransient<IIdentityUserIntegrationService, IdentityUserIntegrationService>();
    }
}
