using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.Calendar.Features;
using SufiChain.SufiPlatform.Calendar.Permissions;
using Volo.Abp.Features;

namespace SufiChain.SufiPlatform.Calendar.Hooshvare.Blazor.Components;

/// <summary>
/// Toolbar launcher that opens the Calendar hooshvare assistant page.
/// Rendered next to the existing <c>CalendarToolbarWidget</c>.
/// </summary>
public partial class CalendarHooshvareLauncher : CalendarHooshvareComponentBase
{
    [Inject]
    protected NavigationManager NavigationManager { get; set; } = default!;

    protected bool CanUseCalendar { get; set; }

    [Inject]
    protected IFeatureChecker FeatureChecker { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        CanUseCalendar = CalendarAssistantChrome.ShowsChrome(
                await FeatureChecker.IsEnabledAsync(CalendarFeatures.Assistant))
            && await IsGrantedAsync(CalendarPermissions.Events.Default);
    }

    protected virtual Task OpenAssistantAsync()
    {
        NavigationManager.NavigateTo("/panel/portal/calendar");
        return Task.CompletedTask;
    }
}
