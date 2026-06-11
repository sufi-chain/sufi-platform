using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using SufiChain.SufiAbp.UI.Navigation;
using MyCompanyName.MyProjectName.Localization;

namespace MyCompanyName.MyProjectName.Blazor.WebSite.Client.Menus;

/// <summary>
/// Menu contributor for the WebSite WebAssembly client.
/// Provides minimal navigation suitable for a public-facing website.
/// </summary>
public class DemoAppPublicMenuContributor : IMenuContributor
{
    private readonly IConfiguration _configuration;

    public DemoAppPublicMenuContributor(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public async Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name == StandardMenus.Main)
        {
            await ConfigureMainMenuAsync(context);
        }
    }

    private Task ConfigureMainMenuAsync(MenuConfigurationContext context)
    {
        var l = context.ServiceProvider.GetRequiredService<IStringLocalizer<DemoAppResource>>();

        // Public site has minimal navigation - just Home for now
        // CMS pages will be added dynamically based on CMS content
        context.Menu.Items.Insert(
            0,
            new ApplicationMenuItem(
                DemoAppPublicMenus.Home,
                l["Menu:Home"],
                "/",
                icon: "fas fa-home",
                order: 0
            )
        );

        return Task.CompletedTask;
    }
}
