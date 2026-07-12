using SufiChain.SufiPlatform.Calendar.Blazor.Public.Components;
using SufiChain.SufiPlatform.UI.Toolbars;

namespace SufiChain.SufiPlatform.Calendar.Blazor.Public;

public class CalendarPublicToolbarContributor : IToolbarContributor
{
    private const string SufiMainToolbar = "SufiMain";

    public virtual Task ConfigureToolbarAsync(IToolbarConfigurationContext context)
    {
        if (context.Toolbar.Name == SufiMainToolbar || context.Toolbar.Name == StandardToolbars.Main)
        {
            context.Toolbar.Items.Add(new ToolbarItem(typeof(CalendarToolbarWidget), order: 190));
        }

        return Task.CompletedTask;
    }
}
