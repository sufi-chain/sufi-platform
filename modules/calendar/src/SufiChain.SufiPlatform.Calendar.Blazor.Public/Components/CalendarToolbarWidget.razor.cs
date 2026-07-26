using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.Calendar.Availability;
using SufiChain.SufiPlatform.Calendar.Calendars;
using SufiChain.SufiPlatform.Calendar.Events;
using SufiChain.SufiPlatform.Calendar.Permissions;
using SufiChain.SufiBlazor.Utilities.DateUtils;
using System.Globalization;

namespace SufiChain.SufiPlatform.Calendar.Blazor.Public.Components;

public partial class CalendarToolbarWidget : CalendarPublicComponentBase
{
    [Inject]
    protected ICalendarEventAppService CalendarEventAppService { get; set; } = default!;

    [Inject]
    protected IAvailabilityCalendarAppService AvailabilityCalendarAppService { get; set; } = default!;

    [Inject]
    protected NavigationManager NavigationManager { get; set; } = default!;

    protected bool IsOpen { get; set; }

    protected bool IsLoading { get; set; }

    protected bool CanUseCalendar { get; set; }

    protected List<EventOccurrenceDto> Agenda { get; } = new();

    protected CalendarDto? PersonalCalendar { get; set; }

    protected string CurrentDateTimeText => FormatDateTime(GetCalendarNow());

    protected override async Task OnInitializedAsync()
    {
        CanUseCalendar =
            await IsGrantedAsync(CalendarPermissions.Calendars.Default) &&
            await IsGrantedAsync(CalendarPermissions.Events.Default);
    }

    protected virtual async Task ToggleMenuAsync()
    {
        await SetOpenAsync(!IsOpen);
    }

    protected virtual async Task SetOpenAsync(bool value)
    {
        if (!CanUseCalendar)
        {
            IsOpen = false;
            return;
        }

        IsOpen = value;
        if (value)
        {
            await LoadAgendaAsync();
        }
    }

    protected virtual Task OpenPageAsync()
    {
        IsOpen = false;
        NavigationManager.NavigateTo("/panel/portal/calendar");
        return Task.CompletedTask;
    }

    protected virtual async Task LoadAgendaAsync()
    {
        IsLoading = true;
        Agenda.Clear();

        PersonalCalendar = await AvailabilityCalendarAppService.GetOrCreateMyPersonalCalendarAsync();
        var timeZone = ResolveCalendarTimeZone();
        var calendarNow = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, timeZone).DateTime;
        var localStart = calendarNow.Date;
        var fromUtc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(localStart, DateTimeKind.Unspecified), timeZone);
        var toUtc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(localStart.AddDays(1), DateTimeKind.Unspecified), timeZone);
        var result = await CalendarEventAppService.GetOccurrencesAsync(PersonalCalendar.Id, new GetOccurrencesInput
        {
            FromUtc = fromUtc,
            ToUtc = toUtc
        });

        Agenda.AddRange(result.Items.OrderBy(x => x.StartUtc));
        IsLoading = false;
    }

    protected virtual string FormatDateTime(DateTime value)
    {
        return SbCalendarHelper.FormatDate(value, null, CultureInfo.CurrentUICulture) + " " + value.ToString("HH:mm", CultureInfo.CurrentUICulture);
    }

    protected virtual string FormatTimeRange(EventOccurrenceDto occurrence)
    {
        var timeZone = ResolveCalendarTimeZone();
        var start = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(occurrence.StartUtc, DateTimeKind.Utc), timeZone);
        var end = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(occurrence.EndUtc, DateTimeKind.Utc), timeZone);
        return $"{start.ToString("HH:mm", CultureInfo.CurrentUICulture)} - {end.ToString("HH:mm", CultureInfo.CurrentUICulture)}";
    }

    protected virtual DateTime GetCalendarNow()
    {
        return TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, ResolveCalendarTimeZone()).DateTime;
    }

    protected virtual TimeZoneInfo ResolveCalendarTimeZone()
    {
        var timeZoneId = PersonalCalendar?.TimeZoneId;
        return string.IsNullOrWhiteSpace(timeZoneId)
            ? TimeZoneInfo.Local
            : TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
    }
}
