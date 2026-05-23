namespace SufiChain.SufiAbp.UI.Navigation;

/// <summary>
/// Options for configuring navigation.
/// </summary>
public class SufiAbpNavigationOptions
{
    /// <summary>
    /// The list of menu contributors that will be invoked to configure menus.
    /// </summary>
    public List<IMenuContributor> MenuContributors { get; }

    /// <summary>
    /// Creates a new NavigationOptions instance.
    /// </summary>
    public SufiAbpNavigationOptions()
    {
        MenuContributors = new List<IMenuContributor>();
    }
}
