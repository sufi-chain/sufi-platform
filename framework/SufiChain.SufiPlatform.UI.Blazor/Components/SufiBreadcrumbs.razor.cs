using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.UI.Layout;

namespace SufiChain.SufiPlatform.UI.Blazor.Components;

/// <summary>
/// Breadcrumb navigation component that displays the current page hierarchy.
/// </summary>
public partial class SufiBreadcrumbs : ComponentBase
{
    [Inject]
    protected IPageLayout PageLayout { get; set; } = default!;

    /// <summary>
    /// Whether to show the home icon as the first item.
    /// </summary>
    [Parameter]
    public bool ShowHome { get; set; } = true;

    /// <summary>
    /// The icon name for the home icon (Sufi Icons).
    /// </summary>
    [Parameter]
    public string HomeIcon { get; set; } = "home";

    /// <summary>
    /// Additional CSS class for the breadcrumb.
    /// </summary>
    [Parameter]
    public string? CssClass { get; set; }
}
