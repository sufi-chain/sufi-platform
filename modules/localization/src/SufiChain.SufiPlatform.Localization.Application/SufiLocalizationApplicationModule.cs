using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SufiChain.SufiPlatform.Localization.ExternalStore;
using Volo.Abp.Application;
using Volo.Abp.Mapperly;
using Volo.Abp.Caching;
using Volo.Abp.Localization;
using Volo.Abp.Localization.External;
using Volo.Abp.Modularity;
using SufiChain.SufiPlatform.Ddd;

namespace SufiChain.SufiPlatform.Localization;

[DependsOn(
    typeof(SufiLocalizationDomainModule),
    typeof(SufiLocalizationApplicationContractsModule),
    typeof(SufiDddApplicationModule),
    typeof(AbpMapperlyModule),
    typeof(AbpCachingModule)
)]
public class SufiLocalizationApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddTransient<ILocalizationTextSeeder, LocalizationTextSeeder>();

        context.Services.AddMapperlyObjectMapper<SufiLocalizationApplicationModule>();

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
