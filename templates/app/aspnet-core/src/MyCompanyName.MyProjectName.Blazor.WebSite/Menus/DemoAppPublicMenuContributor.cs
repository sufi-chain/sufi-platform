using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using SufiChain.SufiPlatform.UI.Navigation;
using MyCompanyName.MyProjectName.Localization;

namespace MyCompanyName.MyProjectName.Blazor.WebSite.Menus;

/// <summary>
/// Menu contributor for the WebSite host.
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
        else if (context.Menu.Name == StandardMenus.User)
        {
            await ConfigureUserMenuAsync(context);
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
    
    private Task ConfigureUserMenuAsync(MenuConfigurationContext context)
    {
        var l = context.ServiceProvider.GetRequiredService<IStringLocalizer<DemoAppResource>>();
        var identityServerUrl = _configuration["AuthServer:Authority"] ?? "";
        
        // Ensure URL ends with /
        if (!string.IsNullOrEmpty(identityServerUrl) && !identityServerUrl.EndsWith('/'))
        {
            identityServerUrl += "/";
        }

        context.Menu.AddItem(new ApplicationMenuItem(
            "Account.Manage", 
            l["Menu:MyAccount"],
            $"{identityServerUrl}Account/Manage?returnUrl={_configuration["App:SelfUrl"]}", 
            icon: "bi bi-gear", 
            order: 1000)
            .RequireAuthenticated());
        
        context.Menu.AddItem(new ApplicationMenuItem(
            "Account.Logout", 
            l["Logout"], 
            url: "~/Account/Logout", 
            icon: "fa fa-power-off", 
            order: int.MaxValue - 1000)
            .RequireAuthenticated());

        return Task.CompletedTask;
    }
}
