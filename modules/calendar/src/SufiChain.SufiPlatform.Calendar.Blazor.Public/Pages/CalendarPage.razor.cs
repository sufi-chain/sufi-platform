using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.Calendar.Availability;
using SufiChain.SufiPlatform.Calendar.Blazor.Public.Components;
using SufiChain.SufiPlatform.Calendar.Events;

namespace SufiChain.SufiPlatform.Calendar.Blazor.Public.Pages;

public partial class CalendarPageBase : CalendarPublicComponentBase
{
    [Inject]
    protected IAvailabilityCalendarAppService AvailabilityCalendarAppService { get; set; } = default!;

    protected Guid? PersonalCalendarId { get; set; }
    protected Guid SelectedCalendarId { get; set; }
    protected SufiCalendarSelect? CalendarSelect { get; set; }
    protected SufiCalendarScheduler? CalendarScheduler { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var personalCalendar = await AvailabilityCalendarAppService.GetOrCreateMyPersonalCalendarAsync();
        PersonalCalendarId = personalCalendar.Id;
        SelectedCalendarId = personalCalendar.Id;
    }

    protected virtual async Task RefreshAsync()
    {
        if (CalendarSelect != null)
        {
            await CalendarSelect.RefreshAsync();
        }

        if (CalendarScheduler != null)
        {
            await CalendarScheduler.RefreshAsync();
        }

        StateHasChanged();
    }

    protected virtual Task OnSelectedCalendarIdChangedAsync(Guid calendarId)
    {
        SelectedCalendarId = calendarId;
        return Task.CompletedTask;
    }

    protected virtual Task OnEventChangedAsync(CalendarEventDto calendarEvent)
    {
        StateHasChanged();
        return Task.CompletedTask;
    }
}
