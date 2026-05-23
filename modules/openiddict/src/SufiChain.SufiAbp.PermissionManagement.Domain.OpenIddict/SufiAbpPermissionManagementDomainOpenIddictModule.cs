using System;
using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.PermissionManagement;
using Volo.Abp.Modularity;
using SufiChain.SufiAbp.OpenIddict;
using SufiChain.SufiAbp.OpenIddict.Applications;

namespace SufiChain.SufiAbp.PermissionManagement.OpenIddict;

[DependsOn(
    typeof(SufiAbpOpenIddictDomainSharedModule),
    typeof(SufiAbpPermissionManagementDomainModule)
)]
public class SufiAbpPermissionManagementDomainOpenIddictModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<PermissionManagementOptions>(options =>
        {
            options.ManagementProviders.Add<ApplicationPermissionManagementProvider>();
            options.ProviderPolicies[ClientPermissionValueProvider.ProviderName] = "OpenIddictPro.Application.ManagePermissions";
        });

        context.Services.AddAbpOptions<PermissionManagementOptions>().PostConfigure<IServiceProvider>((options, serviceProvider) =>
        {
            // The IApplicationFinder implementation in OpenIddict Pro module for tiered application.
            if (serviceProvider.GetService<IApplicationFinder>() == null)
            {
                return;
            }

            options.ResourceManagementProviders.Add<ApplicationResourcePermissionManagementProvider>();
            options.ResourcePermissionProviderKeyLookupServices.Add<ApplicationResourcePermissionProviderKeyLookupService>();
        });
    }
}
