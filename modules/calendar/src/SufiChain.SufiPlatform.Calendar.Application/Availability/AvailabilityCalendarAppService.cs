using Volo.Abp;
using SufiChain.SufiPlatform.Application.Dtos;
using SufiChain.SufiPlatform.Application.Services;
using SufiChain.SufiPlatform.Calendar.Calendars;
using SufiChain.SufiPlatform.Calendar.Permissions;
using SufiChain.SufiPlatform.Data;
using Volo.Abp.Linq;
using Volo.Abp.Data;

namespace SufiChain.SufiPlatform.Calendar.Availability;

public class AvailabilityCalendarAppService : SufiApplicationService, IAvailabilityCalendarAppService
{
    private readonly ICalendarRepository _calendarRepository;
    private readonly CalendarManager _calendarManager;
    private readonly IAsyncQueryableExecuter _asyncExecuter;
    private readonly ICalendarSnapshotProvider _snapshotProvider;
    private readonly BusinessCalendarCalculator _businessCalendarCalculator;
    private readonly CalendarBusinessLocalizationService _businessLocalization;

    public AvailabilityCalendarAppService(
        ICalendarRepository calendarRepository,
        CalendarManager calendarManager,
        IAsyncQueryableExecuter asyncExecuter,
        ICalendarSnapshotProvider snapshotProvider,
        BusinessCalendarCalculator businessCalendarCalculator,
        CalendarBusinessLocalizationService businessLocalization)
    {
        _calendarRepository = calendarRepository;
        _calendarManager = calendarManager;
        _asyncExecuter = asyncExecuter;
        _snapshotProvider = snapshotProvider;
        _businessCalendarCalculator = businessCalendarCalculator;
        _businessLocalization = businessLocalization;
    }

    public virtual async Task<CalendarDto> GetAsync(Guid id)
    {
        await CheckPolicyAsync(CalendarPermissions.Calendars.Default);
        var calendar = await GetVisibleCalendarAsync(id, includeDetails: true);
        return ToDto(calendar);
    }

    public virtual async Task<PagedResultDto<CalendarDto>> GetListAsync(GetCalendarListInput input)
    {
        await CheckPolicyAsync(CalendarPermissions.Calendars.Default);

        var query = await _calendarRepository.WithDetailsAsync();
        query = ApplyVisibilityFilter(query);
        query = ApplyFilter(query, input);
        query = ApplySorting(query, input.Sorting);
        var totalCount = await _asyncExecuter.CountAsync(query);
        var items = await _asyncExecuter.ToListAsync(query.Skip(input.SkipCount).Take(input.MaxResultCount));
        return new PagedResultDto<CalendarDto>(totalCount, items.Select(ToDto).ToList());
    }

    public virtual async Task<ListResultDto<CalendarLookupDto>> GetLookupAsync(CalendarKind? kind = null)
    {
        await CheckPolicyAsync(CalendarPermissions.Calendars.Default);

        var query = await _calendarRepository.GetQueryableAsync();
        query = ApplyVisibilityFilter(query);
        if (kind.HasValue)
        {
            query = query.Where(x => x.Kind == kind.Value);
        }

        var items = await _asyncExecuter.ToListAsync(query);
        return new ListResultDto<CalendarLookupDto>(items.Select(ToLookupDto).ToList());
    }

    public virtual async Task<CalendarDto?> GetDefaultAsync(CalendarKind kind)
    {
        await CheckPolicyAsync(CalendarPermissions.Calendars.Default);
        var calendar = await _calendarRepository.FindDefaultAsync(CurrentTenant.Id, kind);
        return calendar == null || !CanSeeCalendar(calendar) ? null : ToDto(calendar);
    }

    public virtual async Task<CalendarDto> GetOrCreateMyPersonalCalendarAsync()
    {
        await CheckPolicyAsync(CalendarPermissions.Calendars.Default);

        var userId = CurrentUser.Id;
        if (!userId.HasValue)
        {
            throw new BusinessException(CalendarErrorCodes.UserRequired);
        }

        var calendars = await _calendarRepository.GetByOwnerUserIdAsync(CurrentTenant.Id, userId.Value);
        var existing = calendars.FirstOrDefault(x => x.Kind == CalendarKind.Personal);
        if (existing != null)
        {
            return ToDto(existing);
        }

        await CheckPolicyAsync(CalendarPermissions.Calendars.Create);

        var calendar = await _calendarManager.CreateAsync(
            GuidGenerator.Create(),
            CurrentTenant.Id,
            CurrentUser.UserName ?? CurrentUser.Email ?? "Personal Calendar",
            CalendarKind.Personal,
            TimeZoneInfo.Local.Id,
            userId.Value,
            CurrentUser.UserName,
            isDefault: false);

        await _calendarRepository.InsertAsync(calendar, autoSave: true);
        return ToDto(calendar);
    }

    public virtual async Task<ListResultDto<CalendarLookupDto>> GetMyVisibleCalendarsAsync()
    {
        await CheckPolicyAsync(CalendarPermissions.Calendars.Default);

        var query = await _calendarRepository.GetQueryableAsync();
        query = ApplyVisibilityFilter(query);

        var items = await _asyncExecuter.ToListAsync(query);
        return new ListResultDto<CalendarLookupDto>(items.Select(ToLookupDto).ToList());
    }

    public virtual async Task<ListResultDto<CalendarLookupDto>> GetOrganizationUnitCalendarsAsync(List<Guid> organizationUnitIds)
    {
        await CheckPolicyAsync(CalendarPermissions.Calendars.Default);

        if (organizationUnitIds.Count == 0)
        {
            return new ListResultDto<CalendarLookupDto>();
        }

        var query = await _calendarRepository.GetQueryableAsync();
        query = query.Where(x =>
            x.Kind == CalendarKind.Public &&
            x.OwnerUserId.HasValue &&
            organizationUnitIds.Contains(x.OwnerUserId.Value));
        query = ApplyVisibilityFilter(query);

        var items = await _asyncExecuter.ToListAsync(query);
        return new ListResultDto<CalendarLookupDto>(items.Select(ToLookupDto).ToList());
    }

    public virtual async Task<CalendarDto> CreateAsync(CreateUpdateCalendarDto input)
    {
        await CheckPolicyAsync(CalendarPermissions.Calendars.Create);

        var calendar = await _calendarManager.CreateAsync(
            GuidGenerator.Create(),
            CurrentTenant.Id,
            input.Name,
            input.Kind,
            input.TimeZoneId,
            input.OwnerUserId,
            input.OwnerName,
            input.IsDefault,
            input.IsAlwaysOpen,
            string.IsNullOrWhiteSpace(input.Color) ? null : input.Color);

        CopyExtraProperties(input.ExtraProperties, calendar.ExtraProperties);
        await _calendarRepository.InsertAsync(calendar, autoSave: true);
        return ToDto(calendar);
    }

    public virtual async Task<CalendarDto> UpdateAsync(Guid id, CreateUpdateCalendarDto input)
    {
        await CheckPolicyAsync(CalendarPermissions.Calendars.Update);

        var calendar = await _calendarRepository.GetAsync(id, includeDetails: true);
        calendar.SetName(ResolveNameForPersist(calendar.Name, input.Name));
        calendar.SetKind(input.Kind);
        calendar.SetTimeZone(input.TimeZoneId);
        calendar.SetOwner(input.OwnerUserId, input.OwnerName);
        await _calendarManager.SetDefaultAsync(calendar, input.IsDefault);
        calendar.SetAlwaysOpen(input.IsAlwaysOpen);
        calendar.SetColor(string.IsNullOrWhiteSpace(input.Color)
            ? CalendarConsts.GetDefaultColor(input.Kind)
            : input.Color);
        CopyExtraProperties(input.ExtraProperties, calendar.ExtraProperties);

        await _calendarRepository.UpdateAsync(calendar, autoSave: true);
        return ToDto(calendar);
    }

    public virtual async Task DeleteAsync(Guid id)
    {
        await CheckPolicyAsync(CalendarPermissions.Calendars.Delete);
        var calendar = await _calendarRepository.GetAsync(id, includeDetails: true);
        calendar.NotifyChanged();
        await _calendarRepository.DeleteAsync(calendar, autoSave: true);
    }

    public virtual async Task<ListResultDto<WorkingHourRuleDto>> GetWorkingHoursAsync(Guid calendarId)
    {
        await CheckPolicyAsync(CalendarPermissions.Calendars.Default);
        var calendar = await GetVisibleCalendarAsync(calendarId, includeDetails: true);
        return new ListResultDto<WorkingHourRuleDto>(GetOrderedWorkingHourRules(calendar).Select(CalendarDtoMapper.ToDto).ToList());
    }

    public virtual async Task<ListResultDto<WorkingHourRuleDto>> ReplaceWorkingHoursAsync(Guid calendarId, List<CreateUpdateWorkingHourRuleDto> input)
    {
        await CheckPolicyAsync(CalendarPermissions.Calendars.ManageHours);

        var calendar = await _calendarRepository.GetAsync(calendarId, includeDetails: true);
        var rules = input.Select((x, index) => new WorkingHourRule(
            GuidGenerator.Create(),
            calendar.Id,
            x.DayOfWeek,
            TimeOnly.FromTimeSpan(x.StartTime),
            TimeOnly.FromTimeSpan(x.EndTime),
            index)).ToList();
        calendar.ReplaceWorkingHours(rules);
        await _calendarRepository.UpdateAsync(calendar, autoSave: true);
        return new ListResultDto<WorkingHourRuleDto>(GetOrderedWorkingHourRules(calendar).Select(CalendarDtoMapper.ToDto).ToList());
    }

    public virtual async Task<ListResultDto<CalendarExceptionDto>> GetExceptionsAsync(Guid calendarId)
    {
        await CheckPolicyAsync(CalendarPermissions.Calendars.Default);
        var calendar = await GetVisibleCalendarAsync(calendarId, includeDetails: true);
        return new ListResultDto<CalendarExceptionDto>(calendar.Exceptions.Select(CalendarDtoMapper.ToDto).ToList());
    }

    public virtual async Task<ListResultDto<CalendarExceptionDto>> GetEffectiveExceptionsAsync(Guid calendarId)
    {
        await CheckPolicyAsync(CalendarPermissions.Calendars.Default);
        await GetVisibleCalendarAsync(calendarId);

        var snapshot = await _snapshotProvider.GetAsync(calendarId);
        var items = snapshot.Exceptions
            .Select(x => new CalendarExceptionDto
            {
                CalendarId = calendarId,
                Date = x.Date.ToDateTime(TimeOnly.MinValue),
                Kind = x.Kind,
                Description = x.Description,
                Ranges = x.Ranges.Select(r => new WorkingHourRangeDto
                {
                    StartTime = r.StartTime.ToTimeSpan(),
                    EndTime = r.EndTime.ToTimeSpan()
                }).ToList()
            })
            .OrderBy(x => x.Date)
            .ToList();

        return new ListResultDto<CalendarExceptionDto>(items);
    }

    public virtual async Task<ListResultDto<CalendarExceptionDto>> ReplaceExceptionsAsync(Guid calendarId, List<CreateUpdateCalendarExceptionDto> input)
    {
        await CheckPolicyAsync(CalendarPermissions.Calendars.ManageExceptions);

        var calendar = await _calendarRepository.GetAsync(calendarId, includeDetails: true);
        var exceptions = input.Select(x => new CalendarException(
            GuidGenerator.Create(),
            calendar.Id,
            DateOnly.FromDateTime(x.Date),
            x.Kind,
            x.Ranges.Select(r => new WorkingHourRange(TimeOnly.FromTimeSpan(r.StartTime), TimeOnly.FromTimeSpan(r.EndTime))),
            x.Description)).ToList();

        calendar.ReplaceExceptions(exceptions);
        await _calendarRepository.UpdateAsync(calendar, autoSave: true);
        return new ListResultDto<CalendarExceptionDto>(calendar.Exceptions.Select(CalendarDtoMapper.ToDto).ToList());
    }

    public virtual async Task<ListResultDto<CalendarInheritanceDto>> GetInheritancesAsync(Guid calendarId)
    {
        await CheckPolicyAsync(CalendarPermissions.Calendars.Default);
        var calendar = await GetVisibleCalendarAsync(calendarId, includeDetails: true);
        var parentIds = calendar.Inheritances.Select(x => x.ParentCalendarId).ToList();
        var parents = parentIds.Count == 0
            ? new Dictionary<Guid, Calendars.Calendar>()
            : (await _calendarRepository.GetListAsync(x => parentIds.Contains(x.Id))).ToDictionary(x => x.Id);

        return new ListResultDto<CalendarInheritanceDto>(calendar.Inheritances.Select(x =>
        {
            return new CalendarInheritanceDto
            {
                Id = x.Id,
                CalendarId = x.CalendarId,
                ParentCalendarId = x.ParentCalendarId,
                ParentCalendarName = parents.TryGetValue(x.ParentCalendarId, out var parent)
                    ? _businessLocalization.ResolveDisplayName(parent.Name)
                    : null,
                IsInheritedByDefault = x.IsInheritedByDefault
            };
        }).ToList());
    }

    public virtual async Task<CalendarDto> AddInheritanceAsync(Guid calendarId, AddCalendarInheritanceInput input)
    {
        await CheckPolicyAsync(CalendarPermissions.Calendars.Update);
        var calendar = await _calendarRepository.GetAsync(calendarId, includeDetails: true);
        var parentCalendar = await _calendarRepository.GetAsync(input.ParentCalendarId, includeDetails: true);
        await _calendarManager.AddInheritanceAsync(calendar, parentCalendar, input.IsInheritedByDefault);
        await _calendarRepository.UpdateAsync(calendar, autoSave: true);
        return ToDto(calendar);
    }

    public virtual async Task<CalendarInheritanceDto> UpdateInheritanceAsync(Guid calendarId, Guid parentCalendarId, UpdateCalendarInheritanceInput input)
    {
        await CheckPolicyAsync(CalendarPermissions.Calendars.Update);
        var calendar = await _calendarRepository.GetAsync(calendarId, includeDetails: true);
        await _calendarManager.UpdateInheritanceAsync(calendar, parentCalendarId, input.IsInheritedByDefault);
        await _calendarRepository.UpdateAsync(calendar, autoSave: true);

        var inheritance = calendar.Inheritances.First(x => x.ParentCalendarId == parentCalendarId);
        var parentCalendar = await _calendarRepository.FindAsync(parentCalendarId);

        return new CalendarInheritanceDto
        {
            Id = inheritance.Id,
            CalendarId = inheritance.CalendarId,
            ParentCalendarId = inheritance.ParentCalendarId,
            ParentCalendarName = parentCalendar == null
                ? null
                : _businessLocalization.ResolveDisplayName(parentCalendar.Name),
            IsInheritedByDefault = inheritance.IsInheritedByDefault
        };
    }

    public virtual async Task DeleteInheritanceAsync(Guid calendarId, Guid parentCalendarId)
    {
        await CheckPolicyAsync(CalendarPermissions.Calendars.Update);
        var calendar = await _calendarRepository.GetAsync(calendarId, includeDetails: true);
        await _calendarManager.RemoveInheritanceAsync(calendar, parentCalendarId);
        await _calendarRepository.UpdateAsync(calendar, autoSave: true);
    }

    public virtual async Task<ListResultDto<CalendarLookupDto>> GetEligibleParentCalendarsAsync(Guid calendarId)
    {
        await CheckPolicyAsync(CalendarPermissions.Calendars.Default);
        await GetVisibleCalendarAsync(calendarId);

        var query = await _calendarRepository.WithDetailsAsync();
        query = ApplyVisibilityFilter(query);
        var calendars = await _asyncExecuter.ToListAsync(query);

        var existingParentIds = calendars
            .Where(x => x.Id == calendarId)
            .SelectMany(x => x.Inheritances)
            .Select(x => x.ParentCalendarId)
            .ToHashSet();
        var defaultCalendarIds = calendars
            .Where(x => x.Kind == CalendarKind.Default)
            .Select(x => x.Id)
            .ToHashSet();

        var eligible = calendars
            .Where(x => x.Id != calendarId)
            .Where(x => !existingParentIds.Contains(x.Id))
            .Where(x => x.Kind == CalendarKind.Default || x.Inheritances.All(i => defaultCalendarIds.Contains(i.ParentCalendarId)))
            .OrderBy(x => x.Name)
            .Select(ToLookupDto)
            .ToList();

        return new ListResultDto<CalendarLookupDto>(eligible);
    }

    public virtual async Task<TestAvailabilityResultDto> TestAsync(Guid calendarId, TestAvailabilityInput input)
    {
        await CheckPolicyAsync(CalendarPermissions.Calendars.Default);

        await GetVisibleCalendarAsync(calendarId);
        var snapshot = await _snapshotProvider.GetAsync(calendarId);

        return new TestAvailabilityResultDto
        {
            IsOpen = _businessCalendarCalculator.IsOpenAt(snapshot, input.UtcInstant),
            NextOpenAtUtc = _businessCalendarCalculator.NextOpenAt(snapshot, input.UtcInstant),
            NextCloseAtUtc = _businessCalendarCalculator.NextCloseAt(snapshot, input.UtcInstant)
        };
    }

    protected virtual IQueryable<Calendars.Calendar> ApplyFilter(IQueryable<Calendars.Calendar> query, GetCalendarListInput input)
    {
        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            query = query.Where(x => x.Name.Contains(input.Filter));
        }

        if (input.Kind.HasValue)
        {
            query = query.Where(x => x.Kind == input.Kind.Value);
        }

        if (input.OwnerUserId.HasValue)
        {
            query = query.Where(x => x.OwnerUserId == input.OwnerUserId.Value);
        }

        return query;
    }

    protected virtual IQueryable<Calendars.Calendar> ApplySorting(IQueryable<Calendars.Calendar> query, string? sorting)
    {
        if (string.IsNullOrWhiteSpace(sorting))
        {
            return query.OrderBy(x => x.Name).ThenBy(x => x.Id);
        }

        var sortParts = sorting.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        IOrderedQueryable<Calendars.Calendar>? ordered = null;

        foreach (var part in sortParts)
        {
            var tokens = part.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var field = tokens[0];
            var isDescending = tokens.Length > 1 && tokens[1].Equals("DESC", StringComparison.OrdinalIgnoreCase);

            if (ordered == null)
            {
                ordered = (field, isDescending) switch
                {
                    ("Name", false) => query.OrderBy(x => x.Name),
                    ("Name", true) => query.OrderByDescending(x => x.Name),
                    ("Kind", false) => query.OrderBy(x => x.Kind),
                    ("Kind", true) => query.OrderByDescending(x => x.Kind),
                    ("TimeZoneId", false) => query.OrderBy(x => x.TimeZoneId),
                    ("TimeZoneId", true) => query.OrderByDescending(x => x.TimeZoneId),
                    ("IsDefault", false) => query.OrderBy(x => x.IsDefault),
                    ("IsDefault", true) => query.OrderByDescending(x => x.IsDefault),
                    ("IsAlwaysOpen", false) => query.OrderBy(x => x.IsAlwaysOpen),
                    ("IsAlwaysOpen", true) => query.OrderByDescending(x => x.IsAlwaysOpen),
                    ("CreationTime", false) => query.OrderBy(x => x.CreationTime),
                    ("CreationTime", true) => query.OrderByDescending(x => x.CreationTime),
                    _ => query.OrderBy(x => x.Name)
                };
            }
            else
            {
                ordered = (field, isDescending) switch
                {
                    ("Name", false) => ordered.ThenBy(x => x.Name),
                    ("Name", true) => ordered.ThenByDescending(x => x.Name),
                    ("Kind", false) => ordered.ThenBy(x => x.Kind),
                    ("Kind", true) => ordered.ThenByDescending(x => x.Kind),
                    ("TimeZoneId", false) => ordered.ThenBy(x => x.TimeZoneId),
                    ("TimeZoneId", true) => ordered.ThenByDescending(x => x.TimeZoneId),
                    ("IsDefault", false) => ordered.ThenBy(x => x.IsDefault),
                    ("IsDefault", true) => ordered.ThenByDescending(x => x.IsDefault),
                    ("IsAlwaysOpen", false) => ordered.ThenBy(x => x.IsAlwaysOpen),
                    ("IsAlwaysOpen", true) => ordered.ThenByDescending(x => x.IsAlwaysOpen),
                    ("CreationTime", false) => ordered.ThenBy(x => x.CreationTime),
                    ("CreationTime", true) => ordered.ThenByDescending(x => x.CreationTime),
                    _ => ordered.ThenBy(x => x.Name)
                };
            }
        }

        return ordered ?? query.OrderBy(x => x.Name).ThenBy(x => x.Id);
    }

    protected virtual IQueryable<Calendars.Calendar> ApplyVisibilityFilter(IQueryable<Calendars.Calendar> query)
    {
        var userId = CurrentUser.Id;

        return query.Where(x =>
            x.Kind == CalendarKind.Default ||
            x.Kind == CalendarKind.Public ||
            !x.OwnerUserId.HasValue ||
            (userId.HasValue && x.OwnerUserId == userId.Value));
    }

    protected virtual async Task<Calendars.Calendar> GetVisibleCalendarAsync(Guid id, bool includeDetails = false)
    {
        var calendar = await _calendarRepository.GetAsync(id, includeDetails: includeDetails);
        if (!CanSeeCalendar(calendar))
        {
            throw new BusinessException(CalendarErrorCodes.CalendarNotAccessible);
        }

        return calendar;
    }

    protected virtual bool CanSeeCalendar(Calendars.Calendar calendar)
    {
        return calendar.Kind == CalendarKind.Public ||
               calendar.Kind == CalendarKind.Default ||
               !calendar.OwnerUserId.HasValue ||
               (CurrentUser.Id.HasValue && calendar.OwnerUserId == CurrentUser.Id.Value);
    }

    protected virtual void CopyExtraProperties(IDictionary<string, object?> source, IDictionary<string, object?> target)
    {
        target.Clear();
        foreach (var property in source)
        {
            target[property.Key] = property.Value;
        }
    }

    protected virtual IReadOnlyList<WorkingHourRule> GetOrderedWorkingHourRules(Calendars.Calendar calendar)
    {
        return calendar.WorkingHourRules
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Id)
            .ToList();
    }

    protected virtual CalendarDto ToDto(Calendars.Calendar calendar) =>
        CalendarDtoMapper.ToDto(calendar, _businessLocalization);

    protected virtual CalendarLookupDto ToLookupDto(Calendars.Calendar calendar) =>
        CalendarDtoMapper.ToLookupDto(calendar, _businessLocalization);

    /// <summary>
    /// Keeps seeded business-localization keys when the client posts back the resolved display text unchanged.
    /// </summary>
    protected virtual string ResolveNameForPersist(string existingName, string submittedName)
    {
        if (!BusinessLocalizationHelper.IsBusinessLocalizationKey(existingName))
        {
            return submittedName;
        }

        var resolved = _businessLocalization.ResolveDisplayName(existingName);
        if (string.Equals(submittedName, existingName, StringComparison.Ordinal) ||
            string.Equals(submittedName, resolved, StringComparison.Ordinal))
        {
            return existingName;
        }

        return submittedName;
    }
}
