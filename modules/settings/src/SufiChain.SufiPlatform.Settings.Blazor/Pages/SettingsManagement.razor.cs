using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SufiChain.SufiPlatform.Settings.Blazor.Settings;
using SufiChain.SufiPlatform.UI.Layout;
using Volo.Abp.Security.Claims;

namespace SufiChain.SufiPlatform.Settings.Blazor.Pages;

public partial class SettingsManagement : SettingsComponentBase
{
    private static class LoadingKeys
    {
        public const string LoadGroups = "load-groups";
    }

    protected override void OnInitialized()
    {
        LoadingStates[LoadingKeys.LoadGroups] = true;
        SetupPageLayout();
    }

    [Inject] protected IPageLayout PageLayout { get; set; } = default!;
    [Inject] protected IOptions<SettingsComponentOptions> Options { get; set; } = default!;
    [Inject] protected IServiceProvider ServiceProvider { get; set; } = default!;
    [Inject] protected AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;
    [Inject] protected ICurrentPrincipalAccessor PrincipalAccessor { get; set; } = default!;

    private List<SettingComponentGroup> _groups = new();
    private bool _groupsResolved;
    private bool _groupsLoadFailed;

    private bool GroupsBusy => !_groupsResolved || IsOperationLoading(LoadingKeys.LoadGroups);

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
    }

    private Task LoadGroupsAsync()
    {
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
                Logger.LogWarning(exception, "Settings groups could not be loaded.");
            }
            finally
            {
                _groupsResolved = true;
            }
        }, LoadingKeys.LoadGroups);
    }
}
