using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.UI.Toolbars;
using MyCompanyName.MyProjectName.Blazor.WebPublic.Components.Toolbar.LoginLink;

namespace MyCompanyName.MyProjectName.Blazor.WebPublic.Menus;

/// <summary>
/// Toolbar contributor for the WebPublic host.
/// Provides login/logout functionality in the toolbar.
/// </summary>
public class DemoAppPublicToolbarContributor : IToolbarContributor
{
    public Task ConfigureToolbarAsync(IToolbarConfigurationContext context)
    {
        if (context.Toolbar.Name == StandardToolbars.Main)
        {
            // Add login link component for unauthenticated users
            context.Toolbar.Items.Add(new ToolbarItem(typeof(LoginLink)));
        }

        return Task.CompletedTask;
    }
}
