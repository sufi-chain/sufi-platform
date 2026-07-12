using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SufiChain.SufiAbp.LocalizationManagement.ExternalStore;
using Volo.Abp.Application;
using Volo.Abp.Mapperly;
using Volo.Abp.Caching;
using Volo.Abp.Localization;
using Volo.Abp.Localization.External;
using Volo.Abp.Modularity;
using SufiChain.SufiAbp.Ddd;

namespace SufiChain.SufiAbp.LocalizationManagement;

[DependsOn(
    typeof(SufiAbpLocalizationManagementDomainModule),
    typeof(SufiAbpLocalizationManagementApplicationContractsModule),
    typeof(SufiAbpDddApplicationModule),
    typeof(AbpMapperlyModule),
    typeof(AbpCachingModule)
)]
public class SufiAbpLocalizationManagementApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddTransient<ILocalizationTextSeeder, LocalizationTextSeeder>();

        context.Services.AddMapperlyObjectMapper<SufiAbpLocalizationManagementApplicationModule>();

        // Replace the default NullExternalLocalizationStore with our database-backed implementation
        context.Services.Replace(
            ServiceDescriptor.Singleton<IExternalLocalizationStore, DatabaseExternalLocalizationStore>()
        );
    }

    public override void PostConfigureServices(ServiceConfigurationContext context)
    {
        // Add ConfiguredResourceLocalizationContributor to all ABP-configured resources
        // so that database translations can override/extend the embedded JSON translations.
        Configure<AbpLocalizationOptions>(options =>
        {
            foreach (var (_, resource) in options.Resources)
            {
                resource.Contributors.Add(
                    new LazyConfiguredResourceLocalizationContributor(resource.ResourceName));
            }
        });
    }
}
