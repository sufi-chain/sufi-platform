namespace SufiChain.SufiAbp.UI.Toolbars;

/// <summary>
/// Represents a toolbar containing multiple toolbar items.
/// </summary>
public class Toolbar
{
    /// <summary>
    /// The name of the toolbar.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The items in the toolbar.
    /// </summary>
    public List<ToolbarItem> Items { get; }

    /// <summary>
    /// Creates a new Toolbar instance.
    /// </summary>
    /// <param name="name">The toolbar name.</param>
    /// <exception cref="ArgumentNullException">Thrown when name is null.</exception>
    public Toolbar(string name)
    {
        if (name == null)
        {
            throw new ArgumentNullException(nameof(name));
        }
        
        Name = name;
        Items = new List<ToolbarItem>();
    }
}
