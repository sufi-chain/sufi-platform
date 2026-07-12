using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

using Volo.Abp.VirtualFileSystem;
using Volo.Abp.Localization;
namespace SufiChain.SufiPlatform.TextTemplating;

[DependsOn(
    typeof(AbpLocalizationModule),
    typeof(AbpVirtualFileSystemModule)
)]
public class SufiTextTemplatingModule : AbpModule
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

        services.Configure<SufiTextTemplatingOptions>(options =>
        {
            options.DefinitionProviders.AddIfNotContains(definitionProviders);
            options.ContentContributors.AddIfNotContains(contentContributors);
        });
    }
}
