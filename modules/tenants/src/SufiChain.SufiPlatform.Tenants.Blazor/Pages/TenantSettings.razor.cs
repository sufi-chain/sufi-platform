using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
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
/// <c>ICurrentTenant.Change</c> for those calls. Each section save uses that same change.
/// EmailSettingsGroup still opens its test-send dialog; that send uses the same loading helper,
/// so it runs in this tenant scope.
/// </summary>
public partial class TenantSettings : SettingsComponentBase
{
    [Parameter] public Guid TenantId { get; set; }

    [Inject] protected IPageLayout PageLayout { get; set; } = default!;
    [Inject] protected IOptions<SettingsComponentOptions> Options { get; set; } = default!;
    [Inject] protected IServiceProvider ServiceProvider { get; set; } = default!;
    [Inject] protected IStringLocalizer<SufiTenantsResource> TenantLocalizer { get; set; } = default!;
    [Inject] protected AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;
    [Inject] protected ICurrentPrincipalAccessor PrincipalAccessor { get; set; } = default!;

    private static class LoadingKeys
    {
        public const string LoadGroups = "load-groups";
    }

    private List<SettingComponentGroup> _groups = new();
    private bool _groupsResolved;
    private bool _groupsLoadFailed;
    private SettingsTenantScope? _settingsTenantScope;

    private bool GroupsBusy => !_groupsResolved || IsOperationLoading(LoadingKeys.LoadGroups);

    protected override void OnInitialized()
    {
        LoadingStates[LoadingKeys.LoadGroups] = true;
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

    private Task LoadGroupsAsync()
    {
        // Permission checks stay on the host. They decide which groups the host admin may see.
        // Setting values are loaded later by each group, inside the cascaded tenant change.
        _groupsLoadFailed = false;
        if (_groups.Count == 0)
        {
            _groupsResolved = false;
        }

        return ExecuteWithLoadingAsync(async () =>
        {
            try
            {
                using (await SettingComponentAuthorizationScope.EnterAsync(AuthenticationStateProvider, PrincipalAccessor))
                {
                    var groups = await SettingsGroupCatalog.LoadAsync(Options.Value.Contributors, ServiceProvider);
                    _groups = groups.ToList();
                }

                _groupsLoadFailed = false;
            }
            catch (Exception exception)
            {
                _groupsLoadFailed = true;
                _groups = new();
                Logger.LogWarning(exception, "Tenant settings groups could not be loaded.");
            }
            finally
            {
                _groupsResolved = true;
            }
        }, LoadingKeys.LoadGroups);
    }
}
