using System.Collections.ObjectModel;
using System.ComponentModel;
using SufiChain.SufiAbp.UI.PageToolbars;

namespace SufiChain.SufiAbp.UI.Layout;

/// <summary>
/// Interface for managing page layout state including title, breadcrumbs, and toolbar items.
/// </summary>
public interface IPageLayout : INotifyPropertyChanged
{
    /// <summary>
    /// The page title.
    /// </summary>
    string? Title { get; set; }

    /// <summary>
    /// The breadcrumb items for the current page.
    /// </summary>
    ObservableCollection<BreadcrumbItem> BreadcrumbItems { get; }

    /// <summary>
    /// The toolbar items for the current page.
    /// </summary>
    ObservableCollection<PageToolbarItem> ToolbarItems { get; }

    /// <summary>
    /// Custom toolbar content set by pages.
    /// In Blazor, this is typically a RenderFragment.
    /// </summary>
    object? ToolbarContent { get; set; }

    /// <summary>
    /// Resets the page layout state.
    /// </summary>
    void Reset();
}
