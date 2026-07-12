using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.Calendar.FreeBusy;
using SufiChain.SufiBlazor.Utilities.DateUtils;
using System.Globalization;

namespace SufiChain.SufiPlatform.Calendar.Blazor.Public.Components;

public partial class SufiFreeBusyPicker : CalendarPublicComponentBase
{
    [Inject]
    protected IFreeBusyAppService FreeBusyAppService { get; set; } = default!;

    [Parameter]
    public IReadOnlyList<Guid> CalendarIds { get; set; } = Array.Empty<Guid>();

    [Parameter]
    public DateTime FromUtc { get; set; } = DateTime.UtcNow;

    [Parameter]
    public DateTime ToUtc { get; set; } = DateTime.UtcNow.AddDays(7);

    [Parameter]
    public TimeSpan Duration { get; set; } = TimeSpan.FromHours(1);

    [Parameter]
    public string TimeZoneId { get; set; } = TimeZoneInfo.Local.Id;

    [Parameter]
    public EventCallback<FreeBusySlotDto> SlotSelected { get; set; }

    private readonly List<FreeBusySlotDto> _slots = new();
    private SbDateRange? _dateRange;
    private string _durationText = "01:00";
    private CultureInfo _culture = CultureInfo.CurrentUICulture;

    protected override async Task OnParametersSetAsync()
    {
        _culture = CultureInfo.CurrentUICulture;
        var timeZone = ResolveTimeZone();
        var fromLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(FromUtc, DateTimeKind.Utc), timeZone);
        var toLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(ToUtc, DateTimeKind.Utc), timeZone);
        _dateRange = new SbDateRange(DateOnly.FromDateTime(fromLocal), DateOnly.FromDateTime(toLocal));
        _durationText = Duration.ToString(@"hh\:mm");
        await LoadSlotsAsync();
    }

    protected virtual void OnDateRangeChanged(SbDateRange? value)
    {
        _dateRange = value;
    }

    protected virtual async Task LoadSlotsAsync()
    {
        _slots.Clear();
        if (CalendarIds.Count == 0 || _dateRange?.Start is null || _dateRange.End is null || !TimeSpan.TryParse(_durationText, out var duration))
        {
            return;
        }

        await ExecuteWithLoadingAsync(async () =>
        {
            var timeZone = ResolveTimeZone();
            var fromLocal = _dateRange.Start.Value.ToDateTime(TimeOnly.MinValue);
            var toLocal = _dateRange.End.Value.ToDateTime(TimeOnly.MinValue).AddDays(1);
            var result = await FreeBusyAppService.FindAvailableSlotsAsync(new FindAvailableSlotsInput
            {
                CalendarIds = CalendarIds.ToList(),
                FromUtc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(fromLocal, DateTimeKind.Unspecified), timeZone),
                ToUtc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(toLocal, DateTimeKind.Unspecified), timeZone),
                Duration = duration
            });
            _slots.AddRange(result.Items);
        }, LoadingKeys.Load);
    }

    protected virtual async Task SelectSlotAsync(FreeBusySlotDto slot)
    {
        await SlotSelected.InvokeAsync(slot);
    }

    protected virtual string FormatSlot(FreeBusySlotDto slot)
    {
        var timeZone = ResolveTimeZone();
        var start = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(slot.StartUtc, DateTimeKind.Utc), timeZone);
        var end = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(slot.EndUtc, DateTimeKind.Utc), timeZone);
        return $"{SbCalendarHelper.FormatDate(start, null, _culture)} {start:HH:mm} - {end:HH:mm}";
    }

    private TimeZoneInfo ResolveTimeZone()
    {
        return TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);
    }

    private static class LoadingKeys
    {
        public const string Load = "load-free-busy";
    }
}
