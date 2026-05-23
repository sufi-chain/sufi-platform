namespace SufiChain.SufiAbp.UI.PageToolbars;

/// <summary>
/// Represents a page-level toolbar with its contributors.
/// </summary>
/// <remarks>
/// <para>
/// This class is part of the PageToolbar contributor system, which provides an extensibility
/// mechanism for dynamically contributing toolbar items to pages.
/// </para>
/// <para>
/// <b>Current Status:</b> Reserved for future extensibility. Currently, pages use the simpler
/// ChildContent approach with <c>SufiAbpPageToolbar</c> for inline toolbar buttons.
/// </para>
/// <example>
/// <para>Future usage pattern:</para>
/// <code>
/// var toolbar = new PageToolbar();
/// toolbar.Contributors.Add&lt;MyToolbarContributor&gt;();
/// // Pass to SufiAbpPageToolbar.Toolbar parameter
/// </code>
/// </example>
/// </remarks>
public class PageToolbar
{
    /// <summary>
    /// The list of contributors that will configure this toolbar.
    /// </summary>
    public PageToolbarContributorList Contributors { get; set; }

    /// <summary>
    /// Creates a new PageToolbar.
    /// </summary>
    public PageToolbar()
    {
        Contributors = new PageToolbarContributorList();
    }
}
