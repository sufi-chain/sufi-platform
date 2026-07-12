using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Globalization;
using SufiChain.SufiPlatform.Calendar.Availability;
using SufiChain.SufiPlatform.Calendar.Calendars;
using SufiChain.SufiPlatform.Calendar.Permissions;
using SufiChain.SufiPlatform.UI.Layout;
using SufiChain.SufiBlazor.Components.Data;
using SufiChain.SufiBlazor.Contracts.Data;

namespace SufiChain.SufiPlatform.Calendar.Blazor.Pages.Calendar;

public partial class CalendarManagementBase : CalendarComponentBase
{
    public static class LoadingKeys
    {
        public const string LoadCalendars = "load-calendars";
        public const string LoadInheritances = "load-inheritances";
        public const string SaveInheritance = "save-inheritance";
    }

    [Inject] protected IAvailabilityCalendarAppService AvailabilityCalendarAppService { get; set; } = null!;
    [Inject] protected IPageLayout PageLayout { get; set; } = default!;

    protected SbDataGrid<CalendarDto>? _gridRef;
    protected int PageSize { get; set; } = 10;
    protected int PageIndex { get; set; }
    protected long TotalCount { get; set; }
    protected string FilterText { get; set; } = string.Empty;
    protected bool HasActiveFilters => !string.IsNullOrWhiteSpace(FilterText);

    protected bool HasCreatePermission { get; set; }
    protected bool HasEditPermission { get; set; }
    protected bool HasDeletePermission { get; set; }

    protected bool IsEditorOpen { get; set; }
    protected bool IsBusinessHoursOpen { get; set; }
    protected bool IsSchedulerOpen { get; set; }
    protected bool IsDeleteOpen { get; set; }
    protected int EditorActiveTab { get; set; }

    protected Guid EditingCalendarId { get; set; }
    protected CreateUpdateCalendarDto EditingCalendar { get; set; } = new() { TimeZoneId = "UTC" };
    protected CalendarDto? SelectedCalendar { get; set; }
    protected CalendarDto? PendingDeleteCalendar { get; set; }
    protected List<CalendarInheritanceDto> EditingInheritances { get; set; } = new();
    protected List<CalendarLookupDto> EligibleParentCalendars { get; set; } = new();
    protected Dictionary<Guid, CalendarLookupDto> CalendarLookupById { get; set; } = new();
    protected Guid? SelectedParentCalendarId { get; set; }
    protected bool NewInheritanceIsDefault { get; set; }
    protected List<WorkingHourEditorModel> EditingHours { get; set; } = new();
    protected List<ExceptionEditorModel> EditingExceptions { get; set; } = new();
    protected DateOnly? TestDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    protected string TestTimeText { get; set; } = DateTime.UtcNow.ToString("HH:mm");
    protected TestAvailabilityResultDto? TestResult { get; set; }
    protected int BusinessHoursActiveTab { get; set; }
    protected Guid? SchedulerCalendarId { get; set; }

    protected string EditorTitle => EditingCalendarId == Guid.Empty ? L["CreateCalendar"] : L["EditCalendar"];
    protected bool IsEditingExistingCalendar => EditingCalendarId != Guid.Empty;
    protected bool CanManageInheritances => IsEditingExistingCalendar && HasEditPermission;
    protected IReadOnlyList<TimeZoneInfo> TimeZoneOptions { get; } = TimeZoneInfo.GetSystemTimeZones();
    protected IReadOnlyList<CalendarKind> CalendarKindOptions { get; } = Enum.GetValues<CalendarKind>();
    protected IReadOnlyList<CalendarExceptionKind> CalendarExceptionKindOptions { get; } = Enum.GetValues<CalendarExceptionKind>();
    protected IReadOnlyList<DayOfWeek> DayOfWeekOptions { get; } = Enum.GetValues<DayOfWeek>();

    protected override async Task OnInitializedAsync()
    {
        SetupPageLayout();
        await SetPermissionsAsync();
        await base.OnInitializedAsync();
    }

    protected virtual void SetupPageLayout()
    {
        PageLayout.Title = L["Calendar"];
    }

    protected virtual async Task SetPermissionsAsync()
    {
        HasCreatePermission = await AuthorizationService.IsGrantedAsync(CalendarPermissions.Calendars.Create);
        HasEditPermission = await AuthorizationService.IsGrantedAsync(CalendarPermissions.Calendars.Update);
        HasDeletePermission = await AuthorizationService.IsGrantedAsync(CalendarPermissions.Calendars.Delete);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            await ExecuteWithLoadingAsync(
                () => _gridRef?.RefreshDataAsync() ?? Task.CompletedTask,
                LoadingKeys.LoadCalendars);
        }
    }

    protected virtual async Task<SbDataResponse<CalendarDto>> LoadCalendarsDataAsync(SbDataRequest request)
    {
        var result = await AvailabilityCalendarAppService.GetListAsync(new GetCalendarListInput
        {
            Filter = string.IsNullOrWhiteSpace(FilterText) ? null : FilterText,
            Sorting = request.Sorts.Count > 0
                ? string.Join(", ", request.Sorts.Select(sort => sort.Direction == SbSortDirection.Descending ? $"{sort.Field} DESC" : sort.Field))
                : "Name",
            MaxResultCount = request.PageSize,
            SkipCount = Math.Max(0, request.PageIndex * request.PageSize)
        });

        TotalCount = result.TotalCount;
        return new SbDataResponse<CalendarDto>(result.Items, result.TotalCount);
    }

    protected virtual async Task OnPageIndexChangedAsync(int pageIndex)
    {
        PageIndex = pageIndex;
        await ExecuteWithLoadingAsync(
            () => _gridRef?.RefreshDataAsync() ?? Task.CompletedTask,
            LoadingKeys.LoadCalendars);
    }

    protected virtual async Task OnPageSizeChangedAsync(int pageSize)
    {
        PageSize = pageSize;
        PageIndex = 0;
        await ExecuteWithLoadingAsync(
            () => _gridRef?.RefreshDataAsync() ?? Task.CompletedTask,
            LoadingKeys.LoadCalendars);
    }

    protected virtual async Task ApplyFiltersAsync()
    {
        PageIndex = 0;
        await ExecuteWithLoadingAsync(
            () => _gridRef?.RefreshDataAsync() ?? Task.CompletedTask,
            LoadingKeys.LoadCalendars);
    }

    protected virtual async Task ClearFiltersAsync()
    {
        FilterText = string.Empty;
        await ApplyFiltersAsync();
    }

    protected virtual async Task HandleFilterKeyDown(KeyboardEventArgs args)
    {
        if (args.Key == "Enter")
        {
            await ApplyFiltersAsync();
        }
    }

    protected virtual Task OpenCreateModalAsync()
    {
        EditingCalendarId = Guid.Empty;
        EditingCalendar = new CreateUpdateCalendarDto { Kind = CalendarKind.Public, TimeZoneId = GetDefaultTimeZoneId() };
        EditorActiveTab = 0;
        ResetInheritanceEditorState();
        IsEditorOpen = true;
        return Task.CompletedTask;
    }

    protected virtual async Task OpenEditModalAsync(CalendarDto calendar)
    {
        EditingCalendarId = calendar.Id;
        EditingCalendar = new CreateUpdateCalendarDto
        {
            Name = calendar.Name,
            Kind = calendar.Kind,
            TimeZoneId = calendar.TimeZoneId,
            OwnerUserId = calendar.OwnerUserId,
            OwnerName = calendar.OwnerName,
            IsDefault = calendar.IsDefault,
            IsAlwaysOpen = calendar.IsAlwaysOpen,
            ExtraProperties = calendar.ExtraProperties
        };
        EditorActiveTab = 0;
        ResetInheritanceEditorState();
        IsEditorOpen = true;
        await LoadInheritanceEditorDataAsync();
    }

    protected virtual void ResetInheritanceEditorState()
    {
        EditingInheritances = new List<CalendarInheritanceDto>();
        EligibleParentCalendars = new List<CalendarLookupDto>();
        CalendarLookupById = new Dictionary<Guid, CalendarLookupDto>();
        SelectedParentCalendarId = null;
        NewInheritanceIsDefault = false;
    }

    protected virtual async Task LoadInheritanceEditorDataAsync()
    {
        if (!CanManageInheritances)
        {
            return;
        }

        await ExecuteWithLoadingAsync(async () =>
        {
            var inheritancesResult = await AvailabilityCalendarAppService.GetInheritancesAsync(EditingCalendarId);
            var eligibleParentsResult = await AvailabilityCalendarAppService.GetEligibleParentCalendarsAsync(EditingCalendarId);
            var lookupResult = await AvailabilityCalendarAppService.GetLookupAsync();
            EditingInheritances = inheritancesResult.Items.ToList();
            EligibleParentCalendars = eligibleParentsResult.Items.ToList();
            CalendarLookupById = lookupResult.Items.ToDictionary(x => x.Id);
            SelectedParentCalendarId = EligibleParentCalendars.FirstOrDefault()?.Id;
        }, LoadingKeys.LoadInheritances);
    }

    protected virtual Task CloseEditorAsync()
    {
        IsEditorOpen = false;
        EditorActiveTab = 0;
        ResetInheritanceEditorState();
        return Task.CompletedTask;
    }

    protected virtual async Task AddInheritanceAsync()
    {
        if (!CanManageInheritances || !SelectedParentCalendarId.HasValue)
        {
            return;
        }

        try
        {
            await ExecuteWithLoadingAsync(async () =>
            {
                await AvailabilityCalendarAppService.AddInheritanceAsync(EditingCalendarId, new AddCalendarInheritanceInput
                {
                    ParentCalendarId = SelectedParentCalendarId.Value,
                    IsInheritedByDefault = NewInheritanceIsDefault
                });
                await LoadInheritanceEditorDataAsync();
            }, LoadingKeys.SaveInheritance);

            NewInheritanceIsDefault = false;
            await Message.SuccessAsync(L["SavedSuccessfully"]);
            await ExecuteWithLoadingAsync(
                () => _gridRef?.RefreshDataAsync() ?? Task.CompletedTask,
                LoadingKeys.LoadCalendars);
        }
        catch (Exception exception)
        {
            await HandleErrorAsync(exception);
        }
    }

    protected virtual async Task UpdateInheritanceDefaultAsync(CalendarInheritanceDto inheritance, bool isInheritedByDefault)
    {
        if (!CanManageInheritances)
        {
            return;
        }

        try
        {
            await ExecuteWithLoadingAsync(async () =>
            {
                var updated = await AvailabilityCalendarAppService.UpdateInheritanceAsync(
                    EditingCalendarId,
                    inheritance.ParentCalendarId,
                    new UpdateCalendarInheritanceInput { IsInheritedByDefault = isInheritedByDefault });

                var index = EditingInheritances.FindIndex(x => x.ParentCalendarId == inheritance.ParentCalendarId);
                if (index >= 0)
                {
                    EditingInheritances[index] = updated;
                }
            }, LoadingKeys.SaveInheritance);
        }
        catch (Exception exception)
        {
            await HandleErrorAsync(exception);
            await LoadInheritanceEditorDataAsync();
        }
    }

    protected virtual async Task RemoveInheritanceAsync(CalendarInheritanceDto inheritance)
    {
        if (!CanManageInheritances)
        {
            return;
        }

        try
        {
            await ExecuteWithLoadingAsync(async () =>
            {
                await AvailabilityCalendarAppService.DeleteInheritanceAsync(EditingCalendarId, inheritance.ParentCalendarId);
                await LoadInheritanceEditorDataAsync();
            }, LoadingKeys.SaveInheritance);

            await Message.SuccessAsync(L["DeletedSuccessfully"]);
            await ExecuteWithLoadingAsync(
                () => _gridRef?.RefreshDataAsync() ?? Task.CompletedTask,
                LoadingKeys.LoadCalendars);
        }
        catch (Exception exception)
        {
            await HandleErrorAsync(exception);
        }
    }

    protected virtual string GetParentCalendarKindText(Guid parentCalendarId)
    {
        if (!CalendarLookupById.TryGetValue(parentCalendarId, out var parent))
        {
            return string.Empty;
        }

        return GetCalendarKindText(parent.Kind);
    }

    protected virtual string GetParentCalendarDisplayName(CalendarInheritanceDto inheritance)
    {
        if (!string.IsNullOrWhiteSpace(inheritance.ParentCalendarName))
        {
            return inheritance.ParentCalendarName;
        }

        return CalendarLookupById.TryGetValue(inheritance.ParentCalendarId, out var parent)
            ? parent.Name
            : inheritance.ParentCalendarId.ToString();
    }

    protected virtual async Task SaveCalendarAsync()
    {
        try
        {
            if (!await ValidateCalendarEditorAsync())
            {
                return;
            }

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
            await ExecuteWithLoadingAsync(
                () => _gridRef?.RefreshDataAsync() ?? Task.CompletedTask,
                LoadingKeys.LoadCalendars);
        }
        catch (Exception exception)
        {
            await HandleErrorAsync(exception);
        }
    }

    protected virtual async Task OpenBusinessHoursModalAsync(CalendarDto calendar)
    {
        SelectedCalendar = calendar;
        TestResult = null;
        BusinessHoursActiveTab = 0;
        var hoursResult = await AvailabilityCalendarAppService.GetWorkingHoursAsync(calendar.Id);
        var exceptionsResult = await AvailabilityCalendarAppService.GetExceptionsAsync(calendar.Id);
        EditingHours = hoursResult.Items.Select(x => new WorkingHourEditorModel(x)).ToList();
        EditingExceptions = exceptionsResult.Items.Select(x => new ExceptionEditorModel(x)).ToList();
        IsBusinessHoursOpen = true;
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
        if (SelectedCalendar == null)
        {
            return;
        }

        try
        {
            if (!await ValidateWorkingHoursAsync())
            {
                return;
            }

            await AvailabilityCalendarAppService.ReplaceWorkingHoursAsync(SelectedCalendar.Id, EditingHours.Select(x => x.ToDto()).ToList());
            await AvailabilityCalendarAppService.ReplaceExceptionsAsync(SelectedCalendar.Id, EditingExceptions.Select(x => x.ToDto()).ToList());
            IsBusinessHoursOpen = false;
            await Message.SuccessAsync(L["SavedSuccessfully"]);
        }
        catch (Exception exception)
        {
            await HandleErrorAsync(exception);
        }
    }

    protected virtual Task CloseBusinessHoursAsync()
    {
        IsBusinessHoursOpen = false;
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

    protected virtual Task OpenSchedulerModalAsync(CalendarDto calendar)
    {
        SchedulerCalendarId = calendar.Id;
        IsSchedulerOpen = true;
        return Task.CompletedTask;
    }

    protected virtual Task OnSchedulerOpenChangedAsync(bool value)
    {
        IsSchedulerOpen = value;
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
        await ExecuteWithLoadingAsync(
            () => _gridRef?.RefreshDataAsync() ?? Task.CompletedTask,
            LoadingKeys.LoadCalendars);
    }

    protected virtual async Task RefreshAsync()
    {
        await ExecuteWithLoadingAsync(
            () => _gridRef?.RefreshDataAsync() ?? Task.CompletedTask,
            LoadingKeys.LoadCalendars);
    }

    protected virtual string GetDefaultTimeZoneId()
    {
        return TimeZoneOptions.Any(x => x.Id == "Asia/Tehran") ? "Asia/Tehran" : TimeZoneInfo.Local.Id;
    }

    protected virtual string GetCalendarKindText(CalendarKind kind)
    {
        return L[$"Enum:CalendarKind:{kind}"];
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
                if (!TimeSpan.TryParse(hour.StartTimeText, out var start) || !TimeSpan.TryParse(hour.EndTimeText, out var end))
                {
                    await Message.ErrorAsync(L["InvalidTimeRange"]);
                    return false;
                }

                if (end <= start)
                {
                    await Message.ErrorAsync(L["InvalidTimeRange"]);
                    return false;
                }

                ranges.Add((start, end));
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

    protected virtual Task OnCalendarKindChangedAsync(CalendarKind kind)
    {
        EditingCalendar.Kind = kind;
        return Task.CompletedTask;
    }

    protected virtual Task<bool> ValidateCalendarEditorAsync()
    {
        return Task.FromResult(true);
    }

    protected sealed class WorkingHourEditorModel
    {
        public Guid RowKey { get; } = Guid.NewGuid();

        public DayOfWeek DayOfWeek { get; set; } = DayOfWeek.Monday;
        public string StartTimeText { get; set; } = "09:00";
        public string EndTimeText { get; set; } = "17:00";

        public WorkingHourEditorModel()
        {
        }

        public WorkingHourEditorModel(WorkingHourRuleDto dto)
        {
            DayOfWeek = dto.DayOfWeek;
            StartTimeText = dto.StartTime.ToString(@"hh\:mm");
            EndTimeText = dto.EndTime.ToString(@"hh\:mm");
        }

        public CreateUpdateWorkingHourRuleDto ToDto()
        {
            return new CreateUpdateWorkingHourRuleDto
            {
                DayOfWeek = DayOfWeek,
                StartTime = TimeSpan.TryParse(StartTimeText, out var start) ? start : TimeSpan.FromHours(9),
                EndTime = TimeSpan.TryParse(EndTimeText, out var end) ? end : TimeSpan.FromHours(17)
            };
        }
    }

    protected sealed class ExceptionEditorModel
    {
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
