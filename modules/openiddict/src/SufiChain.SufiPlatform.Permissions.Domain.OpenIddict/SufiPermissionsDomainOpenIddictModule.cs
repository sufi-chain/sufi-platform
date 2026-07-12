using System;
using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiPlatform.Permissions;
using Volo.Abp.Modularity;
using SufiChain.SufiPlatform.OpenIddict;
using SufiChain.SufiPlatform.OpenIddict.Applications;

namespace SufiChain.SufiPlatform.Permissions.OpenIddict;

[DependsOn(
    typeof(SufiOpenIddictDomainSharedModule),
    typeof(SufiPermissionsDomainModule)
)]
public class SufiPermissionsDomainOpenIddictModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<PermissionsOptions>(options =>
        {
            options.ManagementProviders.Add<ApplicationPermissionsProvider>();
            options.ProviderPolicies[ClientPermissionValueProvider.ProviderName] = "OpenIddictPro.Application.ManagePermissions";
        });

        context.Services.AddAbpOptions<PermissionsOptions>().PostConfigure<IServiceProvider>((options, serviceProvider) =>
        {
            // The IApplicationFinder implementation in OpenIddict Pro module for tiered application.
            if (serviceProvider.GetService<IApplicationFinder>() == null)
            {
                return;
            }

            options.ResourceManagementProviders.Add<ApplicationResourcePermissionsProvider>();
            options.ResourcePermissionProviderKeyLookupServices.Add<ApplicationResourcePermissionProviderKeyLookupService>();
        });
    }
}
