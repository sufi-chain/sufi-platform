using Microsoft.AspNetCore.Components;
using SufiChain.SufiAbp.Calendar.Availability;
using SufiChain.SufiAbp.Calendar.Events;

namespace SufiChain.SufiAbp.Calendar.Blazor.Public.Pages;

public partial class CalendarPageBase : CalendarPublicComponentBase
{
    [Inject]
    protected IAvailabilityCalendarAppService AvailabilityCalendarAppService { get; set; } = default!;

    protected Guid? PersonalCalendarId { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var personalCalendar = await AvailabilityCalendarAppService.GetOrCreateMyPersonalCalendarAsync();
        PersonalCalendarId = personalCalendar.Id;
    }

    protected virtual Task RefreshAsync()
    {
        StateHasChanged();
        return Task.CompletedTask;
    }

    protected virtual Task OnEventChangedAsync(CalendarEventDto calendarEvent)
    {
        StateHasChanged();
        return Task.CompletedTask;
    }
}
