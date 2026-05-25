using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.Localization;
using SufiChain.SufiAbp.VirtualFileSystem;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.TextTemplating;

[DependsOn(
    typeof(SufiAbpLocalizationModule),
    typeof(SufiAbpVirtualFileSystemModule)
)]
public class SufiAbpTextTemplatingModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        AutoAddProvidersAndContributors(context.Services);
    }

    private static void AutoAddProvidersAndContributors(IServiceCollection services)
    {
        var definitionProviders = new List<Type>();
        var contentContributors = new List<Type>();

        services.OnRegistered(context =>
        {
            if (typeof(ITemplateDefinitionProvider).IsAssignableFrom(context.ImplementationType))
            {
                definitionProviders.Add(context.ImplementationType);
            }

            if (typeof(ITemplateContentContributor).IsAssignableFrom(context.ImplementationType))
            {
                contentContributors.Add(context.ImplementationType);
            }
        });

        services.Configure<SufiAbpTextTemplatingOptions>(options =>
        {
            options.DefinitionProviders.AddIfNotContains(definitionProviders);
            options.ContentContributors.AddIfNotContains(contentContributors);
        });
    }
}
