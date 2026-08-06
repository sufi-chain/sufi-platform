namespace SufiChain.SufiPlatform.UI.PageToolbars;

/// <summary>
/// Interface for contributing items to page toolbars.
/// </summary>
/// <remarks>
/// <para>
/// This interface is part of the PageToolbar contributor system, which provides an extensibility
/// mechanism for dynamically contributing toolbar items to pages.
/// </para>
/// <para>
/// <b>Current Status:</b> Reserved for future extensibility. Currently, pages use the simpler
/// ChildContent approach with <c>SufiPageToolbar</c> for inline toolbar buttons.
/// </para>
/// <para>
/// <b>Usage:</b> When implemented, register contributors with the <see cref="PageToolbar"/>
/// class and pass the PageToolbar instance to SufiPageToolbar.Toolbar parameter.
/// </para>
/// </remarks>
public interface IPageToolbarContributor
{
    /// <summary>
    /// Contributes items to the page toolbar.
    /// </summary>
    /// <param name="context">The contribution context containing the items collection and service provider.</param>
    Task ContributeAsync(PageToolbarContributionContext context);
}
