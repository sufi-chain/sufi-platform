namespace SufiChain.SufiPlatform.UI.Navigation;

/// <summary>
/// Represents an application menu containing menu items.
/// </summary>
public class ApplicationMenu : IHasMenuItems
{
    private string _displayName = default!;

    /// <summary>
    /// Unique name of the menu.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Display name of the menu.
    /// </summary>
    public string DisplayName
    {
        get => _displayName;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("DisplayName cannot be null or whitespace.", nameof(value));
            _displayName = value;
        }
    }

    /// <summary>
    /// Menu items in this menu.
    /// </summary>
    public ApplicationMenuItemList Items { get; }

    /// <summary>
    /// Custom data associated with this menu.
    /// </summary>
    public Dictionary<string, object> CustomData { get; } = new();

    /// <summary>
    /// Creates a new ApplicationMenu.
    /// </summary>
    /// <param name="name">Unique name of the menu.</param>
    /// <param name="displayName">Display name. Defaults to name if not provided.</param>
    public ApplicationMenu(string name, string? displayName = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or whitespace.", nameof(name));

        Name = name;
        DisplayName = displayName ?? name;
        Items = new ApplicationMenuItemList();
    }

    /// <summary>
    /// Adds a menu item to this menu.
    /// </summary>
    public ApplicationMenu AddItem(ApplicationMenuItem menuItem)
    {
        Items.Add(menuItem);
        return this;
    }

    /// <summary>
    /// Adds custom data.
    /// </summary>
    public ApplicationMenu WithCustomData(string key, object value)
    {
        CustomData[key] = value;
        return this;
    }

    public override string ToString()
    {
        return $"[ApplicationMenu] Name = {Name}";
    }
}
