using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiPlatform.Editions.Entitlements;
using SufiChain.SufiPlatform.Features;
using Volo.Abp.Domain;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.Editions;

[DependsOn(
    typeof(AbpDddDomainModule),
    typeof(SufiEditionsDomainSharedModule),
    typeof(SufiFeaturesDomainModule)
)]
public class SufiEditionsDomainModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<FeaturesOptions>(options =>
        {
            options.ProviderPolicies[EditionFeatureValueProvider.ProviderName] =
                EditionsPermissions.Editions.ManageFeatures;
        });

        context.Services.AddTransient<IEntitlementSource, FeatureCheckerEntitlementSource>();
    }
}
