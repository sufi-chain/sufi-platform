namespace SufiChain.SufiAbp.UI.Extensibility.EntityActions;

/// <summary>
/// Represents an action that can be performed on an entity in a data grid.
/// </summary>
public class EntityAction : IEquatable<EntityAction>
{
    /// <summary>
    /// The display text for the action.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// The callback to execute when the action is clicked.
    /// </summary>
    public Func<object, Task> Clicked { get; set; } = _ => Task.CompletedTask;

    /// <summary>
    /// Optional function to get a confirmation message. If returns non-null, shows confirmation dialog.
    /// </summary>
    public Func<object, string?>? ConfirmationMessage { get; set; }

    /// <summary>
    /// Whether this is a primary action.
    /// </summary>
    public bool Primary { get; set; }

    /// <summary>
    /// Optional color for the action button.
    /// </summary>
    public object? Color { get; set; }

    /// <summary>
    /// Optional icon for the action.
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// Function to determine if the action is visible for the entity.
    /// </summary>
    public Func<object, bool>? Visible { get; set; }

    /// <summary>
    /// Whether the action is disabled.
    /// </summary>
    public bool Disabled { get; set; }

    public bool Equals(EntityAction? other)
    {
        return string.Equals(Text, other?.Text, StringComparison.OrdinalIgnoreCase);
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as EntityAction);
    }

    public override int GetHashCode()
    {
        return StringComparer.OrdinalIgnoreCase.GetHashCode(Text);
    }
}
