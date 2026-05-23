using Microsoft.AspNetCore.Components;
using SufiChain.SufiAbp.UI.Layout;
using SufiChain.SufiAbp.UI.PageToolbars;

namespace SufiChain.SufiAbp.UI.Blazor.Components;

/// <summary>
/// Page toolbar component that registers toolbar content with the layout.
/// Title and breadcrumbs are managed by the theme layout based on menu hierarchy.
/// Use ChildContent to provide inline toolbar buttons, or Toolbar to use the PageToolbar system.
/// To customize breadcrumbs, inject IPageLayout and add items to BreadcrumbItems in OnInitializedAsync.
/// </summary>
public partial class SufiAbpPageToolbar : ComponentBase
{
    [Inject]
    protected IPageLayout PageLayout { get; set; } = default!;

    [Inject]
    protected IPageToolbarManager PageToolbarManager { get; set; } = default!;

    private List<RenderFragment> ToolbarItemRenders { get; set; } = new();

    /// <summary>
    /// Optional page toolbar to render using the PageToolbar contributor system.
    /// </summary>
    [Parameter]
    public PageToolbar? Toolbar { get; set; }

    /// <summary>
    /// Inline content to render in the toolbar area (e.g., action buttons).
    /// This content will be displayed in the top bar by the theme layout.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();

        RenderFragment? newToolbarContent = null;

        // Register toolbar content with layout for display in top bar
        if (ChildContent != null)
        {
            newToolbarContent = ChildContent;
        }

        if (Toolbar != null)
        {
            var toolbarItems = await PageToolbarManager.GetItemsAsync(Toolbar);
            
            ToolbarItemRenders.Clear();

            foreach (var item in toolbarItems)
            {
                ToolbarItemRenders.Add(builder =>
                {
                    var sequence = 0;
                    builder.OpenComponent(sequence++, item.ComponentType);
                    if (item.Arguments != null)
                    {
                        foreach (var argument in item.Arguments)
                        {
                            builder.AddAttribute(sequence++, argument.Key, argument.Value);
                        }
                    }
                    builder.CloseComponent();
                });
            }

            // Also register toolbar items with layout for display in top bar
            if (ToolbarItemRenders.Any())
            {
                newToolbarContent = (RenderFragment)(builder =>
                {
                    foreach (var render in ToolbarItemRenders)
                    {
                        render(builder);
                    }
                });
            }
        }

        // Compare against PageLayout.ToolbarContent (actual current state) instead of _lastToolbarContent
        // This handles the case where PageLayout.Reset() cleared the content but this component was reused
        //Console.WriteLine($"[SufiAbpPageToolbar] OnParametersSetAsync: ChildContent={ChildContent?.GetType().Name ?? "null"}, newToolbarContent={newToolbarContent?.GetType().Name ?? "null"}, PageLayout.ToolbarContent={PageLayout.ToolbarContent?.GetType().Name ?? "null"}");
        if (PageLayout.ToolbarContent != newToolbarContent)
        {
           //Console.WriteLine($"[SufiAbpPageToolbar] Setting PageLayout.ToolbarContent = {newToolbarContent?.GetType().Name ?? "null"}");
            PageLayout.ToolbarContent = newToolbarContent;
        }
        else
        {
            //Console.WriteLine("[SufiAbpPageToolbar] PageLayout.ToolbarContent already matches, skipping update");
        }
    }
}
