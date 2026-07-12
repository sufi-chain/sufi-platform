using SufiChain.SufiAbp.Calendar.Blazor.Public.Components;
using SufiChain.SufiAbp.UI.Toolbars;

namespace SufiChain.SufiAbp.Calendar.Blazor.Public;

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
