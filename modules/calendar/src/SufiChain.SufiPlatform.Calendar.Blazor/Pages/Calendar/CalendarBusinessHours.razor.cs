using System.Globalization;
using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.Calendar.Availability;
using SufiChain.SufiPlatform.Calendar.Calendars;
using SufiChain.SufiPlatform.UI.Layout;

namespace SufiChain.SufiPlatform.Calendar.Blazor.Pages.Calendar;

public partial class CalendarBusinessHoursBase : CalendarComponentBase
{
    [Parameter]
    public Guid CalendarId { get; set; }

    [Inject]
    protected IAvailabilityCalendarAppService AvailabilityCalendarAppService { get; set; } = null!;

    [Inject]
    protected IPageLayout PageLayout { get; set; } = default!;

    [Inject]
    protected NavigationManager NavigationManager { get; set; } = default!;

    protected bool IsReady { get; set; }
    protected bool IsLoadFailed { get; set; }
    protected int BusinessHoursActiveTab { get; set; }
    protected List<WorkingHourEditorModel> EditingHours { get; set; } = new();
    protected List<ExceptionEditorModel> EditingExceptions { get; set; } = new();
    protected DateOnly? TestDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    protected string TestTimeText { get; set; } = DateTime.UtcNow.ToString("HH:mm");
    protected TestAvailabilityResultDto? TestResult { get; set; }
    protected IReadOnlyList<CalendarExceptionKind> CalendarExceptionKindOptions { get; } = Enum.GetValues<CalendarExceptionKind>();
    protected IReadOnlyList<DayOfWeek> DayOfWeekOptions { get; } = Enum.GetValues<DayOfWeek>();

    protected override async Task OnInitializedAsync()
    {
        PageLayout.Title = L["BusinessHours"];
        await LoadAsync();
        await base.OnInitializedAsync();
    }

    protected virtual async Task LoadAsync()
    {
        try
        {
            var calendar = await AvailabilityCalendarAppService.GetAsync(CalendarId);
            PageLayout.Title = L["BusinessHours"];
            PageLayout.Subtitle = calendar.Name;
            var hoursResult = await AvailabilityCalendarAppService.GetWorkingHoursAsync(CalendarId);
            var exceptionsResult = await AvailabilityCalendarAppService.GetExceptionsAsync(CalendarId);
            EditingHours = hoursResult.Items.Select(x => new WorkingHourEditorModel(x)).ToList();
            EditingExceptions = exceptionsResult.Items.Select(x => new ExceptionEditorModel(x)).ToList();
            IsReady = true;
        }
        catch (Exception exception)
        {
            IsLoadFailed = true;
            await HandleErrorAsync(exception);
        }
    }

    protected virtual void AddHour()
    {
        EditingHours.Add(new WorkingHourEditorModel());
    }

    protected virtual void RemoveHour(WorkingHourEditorModel rule)
    {
        EditingHours.Remove(rule);
    }

    protected virtual async Task SaveBusinessHoursAsync()
    {
        if (!IsReady)
        {
            return;
        }

        try
        {
            if (!await ValidateWorkingHoursAsync())
            {
                return;
            }

            await AvailabilityCalendarAppService.ReplaceWorkingHoursAsync(CalendarId, EditingHours.Select(x => x.ToDto()).ToList());
            await AvailabilityCalendarAppService.ReplaceExceptionsAsync(CalendarId, EditingExceptions.Select(x => x.ToDto()).ToList());
            await Message.SuccessAsync(L["SavedSuccessfully"]);
            NavigationManager.NavigateTo("/panel/admin/calendar");
        }
        catch (Exception exception)
        {
            await HandleErrorAsync(exception);
        }
    }

    protected virtual Task CancelAsync()
    {
        NavigationManager.NavigateTo("/panel/admin/calendar");
        return Task.CompletedTask;
    }

    protected virtual void AddException()
    {
        EditingExceptions.Add(new ExceptionEditorModel());
    }

    protected virtual void RemoveException(ExceptionEditorModel exception)
    {
        EditingExceptions.Remove(exception);
    }

    protected virtual Task OnTestDateChanged(DateOnly? value)
    {
        TestDate = value;
        return Task.CompletedTask;
    }

    protected virtual async Task RunTestAsync()
    {
        if (!IsReady || TestDate == null || !TimeSpan.TryParse(TestTimeText, out var time))
        {
            return;
        }

        TestResult = await AvailabilityCalendarAppService.TestAsync(CalendarId, new TestAvailabilityInput
        {
            UtcInstant = TestDate.Value.ToDateTime(TimeOnly.MinValue).Add(time)
        });
    }

    protected virtual string GetCalendarExceptionKindText(CalendarExceptionKind kind)
    {
        return L[$"Enum:CalendarExceptionKind:{kind}"];
    }

    protected virtual string GetDayOfWeekText(DayOfWeek dayOfWeek)
    {
        return CultureInfo.CurrentUICulture.DateTimeFormat.GetDayName(dayOfWeek);
    }

    protected virtual Task OnWorkingHourDayChangedAsync(WorkingHourEditorModel rule, DayOfWeek dayOfWeek)
    {
        rule.DayOfWeek = dayOfWeek;
        return Task.CompletedTask;
    }

    protected virtual async Task<bool> ValidateWorkingHoursAsync()
    {
        foreach (var dayGroup in EditingHours.GroupBy(x => x.DayOfWeek))
        {
            var ranges = new List<(TimeSpan Start, TimeSpan End)>();

            foreach (var hour in dayGroup)
            {
                if (hour.StartTime is null || hour.EndTime is null || hour.EndTime <= hour.StartTime)
                {
                    await Message.ErrorAsync(L["InvalidTimeRange"]);
                    return false;
                }

                ranges.Add((hour.StartTime.Value.ToTimeSpan(), hour.EndTime.Value.ToTimeSpan()));
            }

            var orderedRanges = ranges.OrderBy(x => x.Start).ToList();
            for (var index = 1; index < orderedRanges.Count; index++)
            {
                if (orderedRanges[index].Start < orderedRanges[index - 1].End)
                {
                    await Message.ErrorAsync(L["OverlappingWorkingHours"]);
                    return false;
                }
            }
        }

        return true;
    }

    protected sealed class WorkingHourEditorModel
    {
        public Guid RowKey { get; } = Guid.NewGuid();

        public DayOfWeek DayOfWeek { get; set; } = DayOfWeek.Monday;
        public TimeOnly? StartTime { get; set; } = new TimeOnly(9, 0);
        public TimeOnly? EndTime { get; set; } = new TimeOnly(17, 0);

        public WorkingHourEditorModel()
        {
        }

        public WorkingHourEditorModel(WorkingHourRuleDto dto)
        {
            DayOfWeek = dto.DayOfWeek;
            StartTime = ToTimeOnly(dto.StartTime);
            EndTime = ToTimeOnly(dto.EndTime);
        }

        private static TimeOnly? ToTimeOnly(TimeSpan value)
        {
            if (value < TimeSpan.Zero || value >= TimeSpan.FromDays(1))
            {
                return null;
            }

            return TimeOnly.FromTimeSpan(value);
        }

        public CreateUpdateWorkingHourRuleDto ToDto()
        {
            return new CreateUpdateWorkingHourRuleDto
            {
                DayOfWeek = DayOfWeek,
                StartTime = StartTime?.ToTimeSpan() ?? TimeSpan.FromHours(9),
                EndTime = EndTime?.ToTimeSpan() ?? TimeSpan.FromHours(17)
            };
        }
    }

    protected sealed class ExceptionEditorModel
    {
        public Guid RowKey { get; } = Guid.NewGuid();

        public DateOnly? ExceptionDate { get; set; }

        public CalendarExceptionKind Kind { get; set; } = CalendarExceptionKind.Closed;
        public string? Description { get; set; }

        public ExceptionEditorModel()
        {
            ExceptionDate = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        }

        public ExceptionEditorModel(CalendarExceptionDto dto)
        {
            ExceptionDate = DateOnly.FromDateTime(dto.Date);
            Kind = dto.Kind;
            Description = dto.Description;
        }

        public CreateUpdateCalendarExceptionDto ToDto()
        {
            return new CreateUpdateCalendarExceptionDto
            {
                Date = (ExceptionDate ?? DateOnly.FromDateTime(DateTime.UtcNow.Date)).ToDateTime(TimeOnly.MinValue),
                Kind = Kind,
                Description = Description
            };
        }
    }
}
