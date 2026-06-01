using Microsoft.Extensions.DependencyInjection;
using SufiChain.Chat;
using SufiChain.Chat.Blazor.Menus;
using SufiChain.Chat.Blazor.Public;
using SufiChain.SufiAbp.UI.Navigation;
using SufiChain.SufiAbp.UI.Routing;
using Volo.Abp.Modularity;

namespace SufiChain.Chat.Blazor;

[DependsOn(
    typeof(ChatApplicationContractsModule),
    typeof(ChatBlazorPublicModule)
)]
public class ChatBlazorModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<SufiAbpRouterOptions>(options =>
        {
            options.AdditionalAssemblies.Add(typeof(ChatBlazorModule).Assembly);
        });

        Configure<SufiAbpNavigationOptions>(options =>
        {
            options.MenuContributors.Add(new ChatMenuContributor());
        });
    }
}
