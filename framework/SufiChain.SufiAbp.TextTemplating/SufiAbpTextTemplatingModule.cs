using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

using Volo.Abp.VirtualFileSystem;
using Volo.Abp.Localization;
namespace SufiChain.SufiAbp.TextTemplating;

[DependsOn(
    typeof(AbpLocalizationModule),
    typeof(AbpVirtualFileSystemModule)
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
