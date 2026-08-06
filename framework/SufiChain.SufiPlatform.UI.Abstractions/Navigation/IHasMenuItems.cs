namespace SufiChain.SufiPlatform.UI.Navigation;

/// <summary>
/// Interface for objects that have menu items.
/// </summary>
public interface IHasMenuItems
{
    /// <summary>
    /// The collection of menu items.
    /// </summary>
    ApplicationMenuItemList Items { get; }
}
