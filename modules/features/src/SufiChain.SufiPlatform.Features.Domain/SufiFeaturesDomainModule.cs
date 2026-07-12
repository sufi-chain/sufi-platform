using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiPlatform.Features.Localization;
using SufiChain.SufiPlatform.Tenants;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using SufiChain.SufiPlatform.Features;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.Modularity;
using Volo.Abp.Threading;

using Volo.Abp.Caching;
namespace SufiChain.SufiPlatform.Features;

[DependsOn(
    typeof(SufiFeaturesDomainSharedModule),
    typeof(SufiFeaturesModule),
    typeof(AbpCachingModule)
    )]
public class SufiFeaturesDomainModule : AbpModule
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<FeaturesOptions>(options =>
        {
            options.Providers.Add<DefaultValueFeaturesProvider>();
            options.Providers.Add<ConfigurationFeaturesProvider>();
            options.Providers.Add<EditionFeaturesProvider>();

            //TODO: Should be moved to the Tenant Management module
            options.Providers.Add<TenantFeaturesProvider>();
            options.ProviderPolicies[TenantFeatureValueProvider.ProviderName] = TenantsPermissions.Tenants.ManageFeatures;
        });

        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            options.MapCodeNamespace("SufiFeatures", typeof(SufiFeaturesResource));
        });

        if (context.Services.IsDataMigrationEnvironment())
        {
            Configure<FeaturesOptions>(options =>
            {
                options.SaveStaticFeaturesToDatabase = false;
                options.IsDynamicFeatureStoreEnabled = false;
            });
        }
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        AsyncHelper.RunSync(() => OnApplicationInitializationAsync(context));
    }

    public override async Task OnApplicationInitializationAsync(ApplicationInitializationContext context)
    {
        if (context.ServiceProvider.IsDataMigrationEnvironment())
        {
            return;
        }

        var rootServiceProvider = context.ServiceProvider.GetRequiredService<IRootServiceProvider>();
        var initializer = rootServiceProvider.GetService<FeatureDynamicInitializer>();
        if (initializer == null)
        {
            return;
        }

        await initializer.InitializeAsync(true, _cancellationTokenSource.Token);
    }

    public override Task OnApplicationShutdownAsync(ApplicationShutdownContext context)
    {
        _cancellationTokenSource.Cancel();
        return Task.CompletedTask;
    }
}
