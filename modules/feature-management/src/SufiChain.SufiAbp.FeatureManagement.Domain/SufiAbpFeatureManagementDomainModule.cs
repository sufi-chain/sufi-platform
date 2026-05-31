using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.Caching;
using SufiChain.SufiAbp.FeatureManagement.Localization;
using SufiChain.SufiAbp.Features;
using SufiChain.SufiAbp.TenantManagement;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Features;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.Modularity;
using Volo.Abp.Threading;

namespace SufiChain.SufiAbp.FeatureManagement;

[DependsOn(
    typeof(SufiAbpFeatureManagementDomainSharedModule),
    typeof(SufiAbpFeaturesModule),
    typeof(SufiAbpCachingModule)
    )]
public class SufiAbpFeatureManagementDomainModule : AbpModule
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<FeatureManagementOptions>(options =>
        {
            //options.Providers.Add<DefaultValueFeatureManagementProvider>();
            //options.Providers.Add<ConfigurationFeatureManagementProvider>();
            //options.Providers.Add<EditionFeatureManagementProvider>();

            //TODO: Should be moved to the Tenant Management module
            //options.Providers.Add<TenantFeatureManagementProvider>();
            options.ProviderPolicies[TenantFeatureValueProvider.ProviderName] = TenantManagementPermissions.Tenants.ManageFeatures;
        });

        Configure<AbpExceptionLocalizationOptions>(options =>
        {
            options.MapCodeNamespace("SufiAbpFeatureManagement", typeof(SufiAbpFeatureManagementResource));
        });

        if (context.Services.IsDataMigrationEnvironment())
        {
            Configure<FeatureManagementOptions>(options =>
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
