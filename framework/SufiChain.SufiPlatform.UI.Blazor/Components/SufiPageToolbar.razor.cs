using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.UI.Layout;
using SufiChain.SufiPlatform.UI.PageToolbars;

namespace SufiChain.SufiPlatform.UI.Blazor.Components;

/// <summary>
/// Page toolbar component that registers toolbar content with the layout.
/// Title and breadcrumbs are managed by the theme layout based on menu hierarchy.
/// Use ChildContent to provide inline toolbar buttons, or Toolbar to use the PageToolbar system.
/// To customize breadcrumbs, inject IPageLayout and add items to BreadcrumbItems in OnInitializedAsync.
/// </summary>
public partial class SufiPageToolbar : ComponentBase, IDisposable
{
    [Inject]
    protected IPageLayout PageLayout { get; set; } = default!;

    [Inject]
    protected IPageToolbarManager PageToolbarManager { get; set; } = default!;

    private List<RenderFragment> ToolbarItemRenders { get; set; } = new();
    private object? _registeredToolbarContent;

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
                    builder.OpenComponent(0, item.ComponentType);
                    if (item.Arguments != null)
                    {
                        builder.AddMultipleAttributes(1, item.Arguments);
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

        // Always assign. Razor ChildContent is often the same delegate target across
        // parent renders, so Equals() stays true while Disabled/Loading already changed.
        // Skipping here leaves SufiTopBar on the first-render button state.
        _registeredToolbarContent = newToolbarContent;
        PageLayout.ToolbarContent = newToolbarContent;
    }

    public void Dispose()
    {
        if (ReferenceEquals(PageLayout.ToolbarContent, _registeredToolbarContent))
        {
            PageLayout.ToolbarContent = null;
        }

        _registeredToolbarContent = null;
    }
}
