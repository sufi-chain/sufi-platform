using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.Settings.Blazor.Settings;
using SufiChain.SufiPlatform.SufiAI.Blazor.Settings;

namespace SufiChain.SufiPlatform.SufiAI.Blazor.Pages.AI;

public partial class WebResearchSettingsRedirect : AIComponentBase
{
    [Inject]
    protected NavigationManager NavigationManager { get; set; } = default!;

    protected override void OnInitialized()
    {
        NavigationManager.NavigateTo(
            SettingsSectionLink.Redirect(NavigationManager.Uri, WebResearchSettingsSection.Id),
            new NavigationOptions { ReplaceHistoryEntry = true });
    }
}
