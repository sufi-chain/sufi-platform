using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.Settings.Blazor.Settings;
using SufiChain.SufiPlatform.SufiAI.Blazor.Settings;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.SufiAI.Blazor.Pages.AI;

public partial class WebResearchSettingsRedirect : AIComponentBase
{
    [Inject]
    protected NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    protected ICurrentTenant CurrentTenant { get; set; } = default!;

    protected override void OnInitialized()
    {
        // SET-35: tenants must not open the legacy web-research URL or section deep-link.
        if (CurrentTenant.IsAvailable)
        {
            NavigationManager.NavigateTo("/panel/dashboard", new NavigationOptions { ReplaceHistoryEntry = true });
            return;
        }

        NavigationManager.NavigateTo(
            SettingsSectionLink.Redirect(NavigationManager.Uri, WebResearchSettingsSection.Id),
            new NavigationOptions { ReplaceHistoryEntry = true });
    }
}
