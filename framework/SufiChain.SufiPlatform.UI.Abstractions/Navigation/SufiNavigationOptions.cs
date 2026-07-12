namespace SufiChain.SufiPlatform.UI.Navigation;

/// <summary>
/// Options for configuring navigation.
/// </summary>
public class SufiNavigationOptions
{
    /// <summary>
    /// The list of menu contributors that will be invoked to configure menus.
    /// </summary>
    public List<IMenuContributor> MenuContributors { get; }

    /// <summary>
    /// Creates a new NavigationOptions instance.
    /// </summary>
    public SufiNavigationOptions()
    {
        MenuContributors = new List<IMenuContributor>();
    }
}
