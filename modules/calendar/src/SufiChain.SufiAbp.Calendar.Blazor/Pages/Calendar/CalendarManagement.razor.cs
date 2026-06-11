using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using SufiChain.SufiAbp.Calendar.Availability;
using SufiChain.SufiAbp.Calendar.Calendars;
using SufiChain.SufiAbp.Calendar.Permissions;

namespace SufiChain.SufiAbp.Calendar.Blazor.Pages.Calendar;

public partial class CalendarManagementBase : CalendarComponentBase
{
    [Inject] protected IAvailabilityCalendarAppService AvailabilityCalendarAppService { get; set; } = null!;

    protected IReadOnlyList<CalendarDto> CalendarList { get; set; } = Array.Empty<CalendarDto>();
    protected int TotalCount { get; set; }
    protected int PageSize { get; set; } = 10;
    protected int PageIndex { get; set; }
    protected string FilterText { get; set; } = string.Empty;
    protected bool IsLoading { get; set; }

    protected bool HasCreatePermission { get; set; }
    protected bool HasEditPermission { get; set; }
    protected bool HasDeletePermission { get; set; }

    protected bool IsEditorOpen { get; set; }
    protected bool IsHoursOpen { get; set; }
    protected bool IsExceptionsOpen { get; set; }
    protected bool IsTestOpen { get; set; }
    protected bool IsDeleteOpen { get; set; }

    protected Guid EditingCalendarId { get; set; }
    protected CreateUpdateCalendarDto EditingCalendar { get; set; } = new() { TimeZoneId = "UTC" };
    protected CalendarDto? SelectedCalendar { get; set; }
    protected CalendarDto? PendingDeleteCalendar { get; set; }
    protected List<WorkingHourEditorModel> EditingHours { get; set; } = new();
    protected List<ExceptionEditorModel> EditingExceptions { get; set; } = new();
    protected DateOnly? TestDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    protected string TestTimeText { get; set; } = DateTime.UtcNow.ToString("HH:mm");
    protected TestAvailabilityResultDto? TestResult { get; set; }

    protected string EditorTitle => EditingCalendarId == Guid.Empty ? L["CreateCalendar"] : L["EditCalendar"];

    protected string KindText
    {
        get => EditingCalendar.Kind.ToString();
        set => EditingCalendar.Kind = Enum.TryParse<CalendarKind>(value, true, out var kind) ? kind : EditingCalendar.Kind;
    }


    protected int EditorMaxConcurrentValue
    {
        get => EditingCalendar.MaxConcurrent ?? 0;
        set => EditingCalendar.MaxConcurrent = value > 0 ? value : null;
    }

    protected virtual Task OnEditorMaxConcurrentChanged(int value)
    {
        EditorMaxConcurrentValue = value;
        return Task.CompletedTask;
    }

    protected string OwnerTypeText
    {
        get => EditingCalendar.OwnerType.ToString();
        set => EditingCalendar.OwnerType = Enum.TryParse<CalendarOwnerType>(value, true, out var ownerType) ? ownerType : EditingCalendar.OwnerType;
    }

    protected override async Task OnInitializedAsync()
    {
        await SetPermissionsAsync();
        await GetCalendarsAsync();
        await base.OnInitializedAsync();
    }

    protected virtual async Task SetPermissionsAsync()
    {
        HasCreatePermission = await AuthorizationService.IsGrantedAsync(CalendarPermissions.Calendars.Create);
        HasEditPermission = await AuthorizationService.IsGrantedAsync(CalendarPermissions.Calendars.Update);
        HasDeletePermission = await AuthorizationService.IsGrantedAsync(CalendarPermissions.Calendars.Delete);
    }

    protected virtual async Task GetCalendarsAsync()
    {
        try
        {
            IsLoading = true;
            var result = await AvailabilityCalendarAppService.GetListAsync(new GetCalendarListInput
            {
                Filter = string.IsNullOrWhiteSpace(FilterText) ? null : FilterText,
                MaxResultCount = PageSize,
                SkipCount = PageIndex * PageSize
            });
            CalendarList = result.Items;
            TotalCount = (int)result.TotalCount;
        }
        catch (Exception exception)
        {
            await HandleErrorAsync(exception);
        }
        finally
        {
            IsLoading = false;
        }
    }

    protected virtual async Task OnPageIndexChangedAsync(int pageIndex)
    {
        PageIndex = pageIndex;
        await GetCalendarsAsync();
    }

    protected virtual async Task ApplyFiltersAsync()
    {
        PageIndex = 0;
        await GetCalendarsAsync();
    }

    protected virtual async Task ClearFiltersAsync()
    {
        FilterText = string.Empty;
        await ApplyFiltersAsync();
    }

    protected virtual Task OpenCreateModalAsync()
    {
        EditingCalendarId = Guid.Empty;
        EditingCalendar = new CreateUpdateCalendarDto { Kind = CalendarKind.WorkingHours, TimeZoneId = "UTC" };
        IsEditorOpen = true;
        return Task.CompletedTask;
    }

    protected virtual Task OpenEditModalAsync(CalendarDto calendar)
    {
        EditingCalendarId = calendar.Id;
        EditingCalendar = new CreateUpdateCalendarDto
        {
            Name = calendar.Name,
            Kind = calendar.Kind,
            TimeZoneId = calendar.TimeZoneId,
            OwnerType = calendar.OwnerType,
            OwnerId = calendar.OwnerId,
            IsDefault = calendar.IsDefault,
            MaxConcurrent = calendar.MaxConcurrent,
            ExtraProperties = calendar.ExtraProperties
        };
        IsEditorOpen = true;
        return Task.CompletedTask;
    }

    protected virtual Task CloseEditorAsync()
    {
        IsEditorOpen = false;
        return Task.CompletedTask;
    }

    protected virtual async Task SaveCalendarAsync()
    {
        try
        {
            if (EditingCalendarId == Guid.Empty)
            {
                await AvailabilityCalendarAppService.CreateAsync(EditingCalendar);
            }
            else
            {
                await AvailabilityCalendarAppService.UpdateAsync(EditingCalendarId, EditingCalendar);
            }

            IsEditorOpen = false;
            await Message.SuccessAsync(L["SavedSuccessfully"]);
            await GetCalendarsAsync();
        }
        catch (Exception exception)
        {
            await HandleErrorAsync(exception);
        }
    }

    protected virtual async Task OpenHoursModalAsync(CalendarDto calendar)
    {
        SelectedCalendar = calendar;
        var result = await AvailabilityCalendarAppService.GetWorkingHoursAsync(calendar.Id);
        EditingHours = result.Items.Select(x => new WorkingHourEditorModel(x)).ToList();
        IsHoursOpen = true;
    }

    protected virtual void AddHour()
    {
        EditingHours.Add(new WorkingHourEditorModel());
    }

    protected virtual void RemoveHour(WorkingHourEditorModel rule)
    {
        EditingHours.Remove(rule);
    }

    protected virtual async Task SaveHoursAsync()
    {
        if (SelectedCalendar == null)
        {
            return;
        }

        await AvailabilityCalendarAppService.ReplaceWorkingHoursAsync(SelectedCalendar.Id, EditingHours.Select(x => x.ToDto()).ToList());
        IsHoursOpen = false;
        await Message.SuccessAsync(L["SavedSuccessfully"]);
    }

    protected virtual async Task OpenExceptionsModalAsync(CalendarDto calendar)
    {
        SelectedCalendar = calendar;
        var result = await AvailabilityCalendarAppService.GetExceptionsAsync(calendar.Id);
        EditingExceptions = result.Items.Select(x => new ExceptionEditorModel(x)).ToList();
        IsExceptionsOpen = true;
    }

    protected virtual void AddException()
    {
        EditingExceptions.Add(new ExceptionEditorModel());
    }

    protected virtual void RemoveException(ExceptionEditorModel exception)
    {
        EditingExceptions.Remove(exception);
    }

    protected virtual async Task SaveExceptionsAsync()
    {
        if (SelectedCalendar == null)
        {
            return;
        }

        await AvailabilityCalendarAppService.ReplaceExceptionsAsync(SelectedCalendar.Id, EditingExceptions.Select(x => x.ToDto()).ToList());
        IsExceptionsOpen = false;
        await Message.SuccessAsync(L["SavedSuccessfully"]);
    }

    protected virtual Task OpenTestModalAsync(CalendarDto calendar)
    {
        SelectedCalendar = calendar;
        TestResult = null;
        IsTestOpen = true;
        return Task.CompletedTask;
    }

    protected virtual Task OnTestDateChanged(DateOnly? value)
    {
        TestDate = value;
        return Task.CompletedTask;
    }

    protected virtual async Task RunTestAsync()
    {
        if (SelectedCalendar == null || TestDate == null || !TimeSpan.TryParse(TestTimeText, out var time))
        {
            return;
        }

        TestResult = await AvailabilityCalendarAppService.TestAsync(SelectedCalendar.Id, new TestAvailabilityInput
        {
            UtcInstant = TestDate.Value.ToDateTime(TimeOnly.MinValue).Add(time)
        });
    }

    protected virtual Task PromptDeleteAsync(CalendarDto calendar)
    {
        PendingDeleteCalendar = calendar;
        IsDeleteOpen = true;
        return Task.CompletedTask;
    }

    protected virtual Task CancelDeleteAsync()
    {
        PendingDeleteCalendar = null;
        IsDeleteOpen = false;
        return Task.CompletedTask;
    }

    protected virtual async Task DeleteConfirmedAsync()
    {
        if (PendingDeleteCalendar == null)
        {
            return;
        }

        await AvailabilityCalendarAppService.DeleteAsync(PendingDeleteCalendar.Id);
        await CancelDeleteAsync();
        await Message.SuccessAsync(L["DeletedSuccessfully"]);
        await GetCalendarsAsync();
    }

    protected virtual async Task RefreshAsync()
    {
        await GetCalendarsAsync();
    }

    protected sealed class WorkingHourEditorModel
    {
        public string DayOfWeekText { get; set; } = DayOfWeek.Monday.ToString();
        public string StartTimeText { get; set; } = "09:00";
        public string EndTimeText { get; set; } = "17:00";
        public int? MaxConcurrent { get; set; }

        public int MaxConcurrentValue
        {
            get => MaxConcurrent ?? 0;
            set => MaxConcurrent = value > 0 ? value : null;
        }

        public WorkingHourEditorModel()
        {
        }

        public WorkingHourEditorModel(WorkingHourRuleDto dto)
        {
            DayOfWeekText = dto.DayOfWeek.ToString();
            StartTimeText = dto.StartTime.ToString(@"hh\:mm");
            EndTimeText = dto.EndTime.ToString(@"hh\:mm");
            MaxConcurrent = dto.MaxConcurrent;
        }

        public CreateUpdateWorkingHourRuleDto ToDto()
        {
            return new CreateUpdateWorkingHourRuleDto
            {
                DayOfWeek = Enum.TryParse<DayOfWeek>(DayOfWeekText, true, out var day) ? day : DayOfWeek.Monday,
                StartTime = TimeSpan.TryParse(StartTimeText, out var start) ? start : TimeSpan.FromHours(9),
                EndTime = TimeSpan.TryParse(EndTimeText, out var end) ? end : TimeSpan.FromHours(17),
                MaxConcurrent = MaxConcurrent
            };
        }
    }

    protected sealed class ExceptionEditorModel
    {
        public DateOnly? ExceptionDate { get; set; }

        public string KindText { get; set; } = CalendarExceptionKind.Closed.ToString();
        public string? Description { get; set; }

        public ExceptionEditorModel()
        {
            ExceptionDate = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        }

        public ExceptionEditorModel(CalendarExceptionDto dto)
        {
            ExceptionDate = DateOnly.FromDateTime(dto.Date);
            KindText = dto.Kind.ToString();
            Description = dto.Description;
        }

        public CreateUpdateCalendarExceptionDto ToDto()
        {
            return new CreateUpdateCalendarExceptionDto
            {
                Date = (ExceptionDate ?? DateOnly.FromDateTime(DateTime.UtcNow.Date)).ToDateTime(TimeOnly.MinValue),
                Kind = Enum.TryParse<CalendarExceptionKind>(KindText, true, out var kind) ? kind : CalendarExceptionKind.Closed,
                Description = Description
            };
        }
    }
}
