using System.Diagnostics;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace SufiChain.SufiPlatform.Settings.Blazor.Settings;

/// <summary>
/// Interface for contributing setting groups to the settings page.
/// </summary>
public interface ISettingComponentContributor
{
    /// <summary>
    /// Configures the setting group.
    /// </summary>
    Task ConfigureAsync(SettingComponentCreationContext context);

    /// <summary>
    /// Checks if the current user has permission to view this setting group.
    /// </summary>
    Task<bool> CheckPermissionsAsync(SettingComponentCreationContext context);
}

/// <summary>
/// Context for creating setting components.
/// Settings are managed for the current context using Application Services (DDD-compliant).
/// </summary>
public class SettingComponentCreationContext
{
    public IServiceProvider ServiceProvider { get; }
    public List<SettingComponentGroup> Groups { get; } = new();

    public SettingComponentCreationContext(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    public T GetRequiredService<T>() where T : notnull
    {
        return ServiceProvider.GetRequiredService<T>();
    }

    public T? GetService<T>()
    {
        return ServiceProvider.GetService<T>();
    }

    /// <summary>
    /// Normalizes the groups by ordering them.
    /// </summary>
    public void Normalize()
    {
        foreach (var duplicate in Groups.GroupBy(group => group.Order).Where(group => group.Count() > 1))
        {
            var ids = string.Join(", ", duplicate.Select(group => group.Id));
            Trace.TraceWarning(
                "Settings groups share Order {0}: {1}. Each group on a page needs a unique order.",
                duplicate.Key,
                ids);
        }

        var orderedGroups = Groups.OrderBy(g => g.Order).ToList();
        Groups.Clear();
        Groups.AddRange(orderedGroups);
    }
}

/// <summary>
/// Represents a setting group displayed as a settings section.
/// </summary>
public class SettingComponentGroup
{
    /// <summary>
    /// Unique identifier for the group. Used as the <c>?section=</c> value.
    /// </summary>
    public string Id { get; set; } = default!;

    /// <summary>
    /// Display name for the group.
    /// </summary>
    public string DisplayName { get; set; } = default!;

    /// <summary>
    /// Registered SufiIcons name for the settings rail.
    /// </summary>
    public required string Icon { get; set; }

    /// <summary>
    /// Optional one-line description under the section heading.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The component type to render for this group.
    /// </summary>
    public Type ComponentType { get; set; } = default!;

    /// <summary>
    /// Optional parameter to pass to the component.
    /// </summary>
    public object? Parameter { get; set; }

    /// <summary>
    /// Permission that must be granted before the section is shown.
    /// A denied or failed check hides the section.
    /// Null means the contributor already decided visibility, including an OR of several permissions.
    /// </summary>
    public string? Policy { get; set; }

    /// <summary>
    /// Order of the group. Each group on a page needs a unique order. Default is 1000.
    /// </summary>
    public int Order { get; set; } = 1000;
}

/// <summary>
/// Options for setting management component contributors.
/// </summary>
public class SettingsComponentOptions
{
    public List<ISettingComponentContributor> Contributors { get; } = new();
}

/// <summary>
/// Interface for setting group components that support centralized save from modal/page footer.
/// Implement this interface in your setting group component to allow the parent to trigger save.
/// </summary>
public interface ISaveableSettingGroup
{
    /// <summary>
    /// Saves the settings in this group.
    /// Called by the parent modal/page when the user clicks the Save button.
    /// </summary>
    Task SaveAsync();
    
    /// <summary>
    /// Gets a value indicating whether the save operation is currently in progress.
    /// Used to show loading state on the Save button.
    /// </summary>
    bool IsSaving { get; }
}

/// <summary>
/// A setting group that reports unsaved edits and can restore the last saved values.
/// The settings layout saves and discards one section at a time.
/// </summary>
public interface IEditableSettingGroup : ISaveableSettingGroup
{
    /// <summary>
    /// True after the user changes a value and before the next successful save or discard.
    /// </summary>
    bool HasUnsavedChanges { get; }

    /// <summary>
    /// Raised when <see cref="HasUnsavedChanges"/> changes.
    /// </summary>
    event Action? EditStateChanged;

    /// <summary>
    /// Saves this section. Returns false when the save fails. Does not show a success toast.
    /// </summary>
    Task<bool> TrySaveAsync();

    /// <summary>
    /// Restores the last loaded or saved values.
    /// </summary>
    Task DiscardAsync();
}
