using SufiChain.SufiAbp.UI.Toolbars;
using Volo.Abp.Users;

namespace MyCompanyName.MyProjectName.Menus;

public class DemoAppToolbarContributor : IToolbarContributor
{
    public virtual Task ConfigureToolbarAsync(IToolbarConfigurationContext context)
    {
        if (context.Toolbar.Name != StandardToolbars.Main)
        {
            return Task.CompletedTask;
        }

        if (!context.ServiceProvider.GetRequiredService<ICurrentUser>().IsAuthenticated)
        {
            // Use Blazor component for toolbar items
            context.Toolbar.Items.Add(new ToolbarItem(typeof(Blazor.WebApp.Components.Toolbar.LoginLink.LoginLink)));
        }

        return Task.CompletedTask;
    }
}
