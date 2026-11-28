using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SufiChain.SufiPlatform.Data;
using Volo.Abp.Application;
using Volo.Abp.Mapperly;
using SufiChain.SufiPlatform.FileManager.AccessControl;
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
        context.Services.AddTransient<IOrganizationUnitIntegrationService, OrganizationUnitIntegrationService>();
        context.Services.AddTransient<IIdentitySettingsIntegrationService, IdentitySettingsIntegrationService>();

        context.Services.Replace(ServiceDescriptor.Scoped<IRoleNameToIdResolver, IdentityRoleNameToIdResolver>());
        context.Services.Replace(ServiceDescriptor.Transient<IFolderGrantPrincipalStore, IdentityFolderGrantPrincipalStore>());
    }
}
