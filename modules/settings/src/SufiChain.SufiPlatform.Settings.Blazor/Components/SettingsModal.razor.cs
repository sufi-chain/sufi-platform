using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using SufiChain.SufiPlatform.Settings.Blazor.Settings;
using SufiChain.SufiPlatform.Settings.Localization;
using SufiChain.SufiPlatform.UI.Blazor;

namespace SufiChain.SufiPlatform.Settings.Blazor.Components;

/// <summary>
/// Modal component for managing settings with vertical tab navigation.
/// Matches the pattern used by PermissionsModal and FeaturesModal.
/// Settings are managed for the current context using Application Services (DDD-compliant).
/// </summary>
public partial class SettingsModal : SettingsComponentBase
{

    private static class LoadingKeys
    {
        public const string LoadGroups = "load-groups";
        public const string Save = "save";
    }

    [Inject] private IOptions<SettingsComponentOptions> Options { get; set; } = default!;
    [Inject] private IServiceProvider ServiceProvider { get; set; } = default!;

    private bool _isOpen;
    private List<SettingComponentGroup> _groups = new();
    private string? _selectedTabId;
    private DynamicComponent? _currentComponentRef;

    /// <summary>
    /// Opens the setting management modal for the current context.
    /// </summary>
    public async Task OpenAsync()
    {
        _selectedTabId = null;

        await ExecuteWithLoadingAsync(async () =>
        {
            var context = new SettingComponentCreationContext(ServiceProvider);

            foreach (var contributor in Options.Value.Contributors)
            {
                if (await contributor.CheckPermissionsAsync(context))
                {
                    await contributor.ConfigureAsync(context);
                }
            }

            context.Normalize();
            _groups = context.Groups;

            // Select first tab if not already selected
            if (_groups.Count > 0 && string.IsNullOrEmpty(_selectedTabId))
            {
                _selectedTabId = GetNormalizedGroupId(_groups.First().Id);
            }
        }, LoadingKeys.LoadGroups);

        _isOpen = true;
        StateHasChanged();
    }

    private void SelectGroup(string groupId)
    {
        _selectedTabId = groupId;
        StateHasChanged();
    }

    private string GetNormalizedGroupId(string id)
    {
        return "SettingGroup_" + id.Replace(".", "_");
    }

    private Dictionary<string, object?>? GetComponentParameters(SettingComponentGroup group)
    {
        if (group.Parameter == null)
        {
            return null;
        }

        return new Dictionary<string, object?>
        {
            { "Parameter", group.Parameter }
        };
    }

    private void Hide()
    {
        _selectedTabId = null;
        _isOpen = false;
        StateHasChanged();
    }

    private Task SaveAsync() => ExecuteWithLoadingAsync(async () =>
    {
        // Get the current component instance from DynamicComponent
        if (_currentComponentRef?.Instance is ISaveableSettingGroup saveableComponent)
        {
            await saveableComponent.SaveAsync();
        }
        else
        {
            // Fallback: notify user that saving is not supported for this component
            await Notify.WarnAsync(L["SaveNotSupportedForThisSettingGroup"]);
        }
    }, LoadingKeys.Save);
}
