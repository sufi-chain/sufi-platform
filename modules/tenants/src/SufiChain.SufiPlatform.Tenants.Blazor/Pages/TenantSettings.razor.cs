using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using SufiChain.SufiPlatform.Settings.Blazor;
using SufiChain.SufiPlatform.Settings.Blazor.Settings;
using SufiChain.SufiPlatform.Tenants.Localization;
using SufiChain.SufiPlatform.UI.Blazor;
using SufiChain.SufiPlatform.UI.Layout;
using Volo.Abp.Security.Claims;

namespace SufiChain.SufiPlatform.Tenants.Blazor.Pages;

/// <summary>
/// Host page for one tenant's settings. Setting application services persist through the
/// ambient tenant: <c>SetForTenantOrGlobalAsync</c> and <c>SetForCurrentTenantAsync</c> write
/// the tenant store when the id is set, and do not call <c>SetGlobalAsync</c>. Changing from
/// the host to <see cref="TenantId"/> therefore does not overwrite host setting rows.
/// The editor is shown only while the workspace is the host. An active tenant is refused
/// instead of changing from one tenant to another.
/// Group value loads run in each group's own <c>ExecuteWithLoadingAsync</c> after render, so
/// <see cref="SettingsTenantScope"/> is cascaded and <see cref="SufiComponentBase"/> enters
/// <c>ICurrentTenant.Change</c> for those calls. Save enters the same change here.
/// EmailSettingsGroup still opens its test-send dialog; that send uses the same loading helper,
/// so it runs in this tenant scope.
/// </summary>
public partial class TenantSettings : SettingsComponentBase
{
    private static class LoadingKeys
    {
        public const string LoadGroups = "load-groups";
        public const string Save = "save";
    }

    [Parameter] public Guid TenantId { get; set; }

    [Inject] protected IPageLayout PageLayout { get; set; } = default!;
    [Inject] protected IOptions<SettingsComponentOptions> Options { get; set; } = default!;
    [Inject] protected IServiceProvider ServiceProvider { get; set; } = default!;
    [Inject] protected IStringLocalizer<SufiTenantsResource> TenantLocalizer { get; set; } = default!;
    [Inject] protected AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;
    [Inject] protected ICurrentPrincipalAccessor PrincipalAccessor { get; set; } = default!;

    private List<SettingComponentGroup> _groups = new();
    private string? _selectedTabId;
    private DynamicComponent? _currentComponentRef;
    private SettingsTenantScope? _settingsTenantScope;
    private SettingComponentGroup? SelectedGroup => _groups.FirstOrDefault(group => group.Id == _selectedTabId);

    protected override void OnInitialized()
    {
        SetupPageLayout();
    }

    protected override void OnParametersSet()
    {
        _settingsTenantScope = CurrentTenant.Id == null
            ? new SettingsTenantScope(TenantId)
            : null;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender && CurrentTenant.Id == null)
        {
            await LoadGroupsAsync();
        }
    }

    private void SetupPageLayout()
    {
        PageLayout.Title = L["Settings"];
    }

    private Task LoadGroupsAsync() => ExecuteWithLoadingAsync(async () =>
    {
        // Permission checks stay on the host. They decide which groups the host admin may see.
        // Setting values are loaded later by each group, inside the cascaded tenant change.
        using (await SettingComponentAuthorizationScope.EnterAsync(AuthenticationStateProvider, PrincipalAccessor))
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
        }

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

    private Task SaveAsync() => IsOperationLoading(LoadingKeys.Save) || IsOperationLoading(LoadingKeys.LoadGroups) || CurrentTenant.Id != null
        ? Task.CompletedTask
        : ExecuteWithLoadingAsync(async () =>
    {
        using (CurrentTenant.Change(TenantId))
        {
            if (_currentComponentRef?.Instance is ISaveableSettingGroup saveableComponent)
            {
                await saveableComponent.SaveAsync();
            }
            else
            {
                await Notify.WarnAsync(L["SaveNotSupportedForThisSettingGroup"]);
            }
        }
    }, LoadingKeys.Save);
}
