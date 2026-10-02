using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using SufiChain.SufiPlatform.Calendar.Availability;
using SufiChain.SufiPlatform.Calendar.Calendars;
using SufiChain.SufiPlatform.Calendar.Permissions;
using SufiChain.SufiPlatform.UI.Layout;
using SufiChain.SufiBlazor.Components.Data;
using SufiChain.SufiBlazor.Components.Overlays;
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
    [Inject] protected NavigationManager NavigationManager { get; set; } = default!;

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
    protected bool IsSchedulerOpen { get; set; }
    protected SbConfirmDialog? DeleteConfirmDialog;
    protected int EditorActiveTab { get; set; }

    protected Guid EditingCalendarId { get; set; }
    protected CreateUpdateCalendarDto EditingCalendar { get; set; } = new() { TimeZoneId = "UTC" };
    protected CalendarDto? PendingDeleteCalendar { get; set; }
    protected List<CalendarInheritanceDto> EditingInheritances { get; set; } = new();
    protected List<CalendarLookupDto> EligibleParentCalendars { get; set; } = new();
    protected Dictionary<Guid, CalendarLookupDto> CalendarLookupById { get; set; } = new();
    protected Guid? SelectedParentCalendarId { get; set; }
    protected bool NewInheritanceIsDefault { get; set; }
    protected Guid? SchedulerCalendarId { get; set; }

    protected string EditorTitle => EditingCalendarId == Guid.Empty ? L["CreateCalendar"] : L["EditCalendar"];
    protected bool IsEditingExistingCalendar => EditingCalendarId != Guid.Empty;
    protected bool CanManageInheritances => IsEditingExistingCalendar && HasEditPermission;
    protected IReadOnlyList<TimeZoneInfo> TimeZoneOptions { get; } = TimeZoneInfo.GetSystemTimeZones();
    protected IReadOnlyList<CalendarKind> CalendarKindOptions { get; } = Enum.GetValues<CalendarKind>();

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
        EditingCalendar = new CreateUpdateCalendarDto
        {
            Kind = CalendarKind.Public,
            TimeZoneId = GetDefaultTimeZoneId(),
            Color = CalendarConsts.GetDefaultColor(CalendarKind.Public)
        };
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
            Color = string.IsNullOrWhiteSpace(calendar.Color)
                ? CalendarConsts.GetDefaultColor(calendar.Kind)
                : calendar.Color,
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

    protected virtual Task OpenBusinessHoursAsync(CalendarDto calendar)
    {
        NavigationManager.NavigateTo($"/panel/admin/calendar/business-hours/{calendar.Id:D}");
        return Task.CompletedTask;
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

    protected virtual Task PromptDeleteAsync(CalendarDto calendar)
    {
        PendingDeleteCalendar = calendar;
        DeleteConfirmDialog?.Show();
        return Task.CompletedTask;
    }

    protected virtual Task CancelDeleteAsync()
    {
        PendingDeleteCalendar = null;
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

    protected virtual Task OnCalendarKindChangedAsync(CalendarKind kind)
    {
        var previousDefault = CalendarConsts.GetDefaultColor(EditingCalendar.Kind);
        EditingCalendar.Kind = kind;
        if (string.IsNullOrWhiteSpace(EditingCalendar.Color) ||
            string.Equals(EditingCalendar.Color, previousDefault, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(EditingCalendar.Color, CalendarConsts.DefaultColor, StringComparison.OrdinalIgnoreCase))
        {
            EditingCalendar.Color = CalendarConsts.GetDefaultColor(kind);
        }

        return Task.CompletedTask;
    }

    protected virtual Task<bool> ValidateCalendarEditorAsync()
    {
        return Task.FromResult(true);
    }
}
