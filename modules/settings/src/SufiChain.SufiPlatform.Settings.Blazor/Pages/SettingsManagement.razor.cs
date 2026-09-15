using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using SufiChain.SufiPlatform.UI.Layout;
using SufiChain.SufiPlatform.Settings.Blazor.Settings;

namespace SufiChain.SufiPlatform.Settings.Blazor.Pages;

public partial class SettingsManagement : SettingsComponentBase
{
    protected override void OnInitialized()
    {
        SetupPageLayout();
    }

    private static class LoadingKeys
    {
        public const string LoadGroups = "load-groups";
        public const string Save = "save";
    }

    [Inject] protected IPageLayout PageLayout { get; set; } = default!;
    [Inject] protected IOptions<SettingsComponentOptions> Options { get; set; } = default!;
    [Inject] protected IServiceProvider ServiceProvider { get; set; } = default!;

    private List<SettingComponentGroup> _groups = new();
    private string? _selectedTabId;
    private DynamicComponent? _currentComponentRef;
    private SettingComponentGroup? SelectedGroup => _groups.FirstOrDefault(group => group.Id == _selectedTabId);


    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        
        if (firstRender)
        {
            await LoadGroupsAsync();
        }
    }

    private void SetupPageLayout()
    {
        PageLayout.Title = L["Settings"];
        // Breadcrumbs are auto-generated from menu hierarchy by the layout
    }

    private Task LoadGroupsAsync() => ExecuteWithLoadingAsync(async () =>
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
            _selectedTabId = _groups.First().Id;
        }
    }, LoadingKeys.LoadGroups);

    private void SelectGroup(string groupId)
    {
        if (IsOperationLoading(LoadingKeys.Save) || groupId == _selectedTabId)
        {
            return;
        }

        _currentComponentRef = null;
        _selectedTabId = groupId;
        StateHasChanged();
    }

    private static Dictionary<string, object?>? GetComponentParameters(SettingComponentGroup group)
    {
        return group.Parameter == null ? null : new Dictionary<string, object?> { ["Parameter"] = group.Parameter };
    }

    private Task SaveAsync() => IsOperationLoading(LoadingKeys.Save) || IsOperationLoading(LoadingKeys.LoadGroups)
        ? Task.CompletedTask
        : ExecuteWithLoadingAsync(async () =>
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
