using SufiChain.SufiAbp.Application.Dtos;
using SufiChain.SufiAbp.Application.Services;
using SufiChain.SufiAbp.Calendar.Calendars;
using SufiChain.SufiAbp.Calendar.Permissions;
using SufiChain.SufiAbp.Linq;

namespace SufiChain.SufiAbp.Calendar.Availability;

public class AvailabilityCalendarAppService : SufiAbpApplicationService, IAvailabilityCalendarAppService
{
    private readonly ICalendarRepository _calendarRepository;
    private readonly CalendarManager _calendarManager;
    private readonly IAsyncQueryableExecuter _asyncExecuter;
    private readonly ICalendarSnapshotProvider _snapshotProvider;
    private readonly BusinessCalendarCalculator _businessCalendarCalculator;

    public AvailabilityCalendarAppService(
        ICalendarRepository calendarRepository,
        CalendarManager calendarManager,
        IAsyncQueryableExecuter asyncExecuter,
        ICalendarSnapshotProvider snapshotProvider,
        BusinessCalendarCalculator businessCalendarCalculator)
    {
        _calendarRepository = calendarRepository;
        _calendarManager = calendarManager;
        _asyncExecuter = asyncExecuter;
        _snapshotProvider = snapshotProvider;
        _businessCalendarCalculator = businessCalendarCalculator;
    }

    public virtual async Task<CalendarDto> GetAsync(Guid id)
    {
        await CheckPolicyAsync(CalendarPermissions.Calendars.Default);
        return CalendarDtoMapper.ToDto(await _calendarRepository.GetAsync(id, includeDetails: true));
    }

    public virtual async Task<PagedResultDto<CalendarDto>> GetListAsync(GetCalendarListInput input)
    {
        await CheckPolicyAsync(CalendarPermissions.Calendars.Default);

        var query = await _calendarRepository.GetQueryableAsync();
        query = ApplyFilter(query, input);
        var totalCount = await _asyncExecuter.CountAsync(query);
        var items = await _asyncExecuter.ToListAsync(query.Skip(input.SkipCount).Take(input.MaxResultCount));
        return new PagedResultDto<CalendarDto>(totalCount, items.Select(CalendarDtoMapper.ToDto).ToList());
    }

    public virtual async Task<ListResultDto<CalendarLookupDto>> GetLookupAsync(CalendarKind? kind = null)
    {
        await CheckPolicyAsync(CalendarPermissions.Calendars.Default);

        var query = await _calendarRepository.GetQueryableAsync();
        if (kind.HasValue)
        {
            query = query.Where(x => x.Kind == kind.Value);
        }

        var items = await _asyncExecuter.ToListAsync(query);
        return new ListResultDto<CalendarLookupDto>(items.Select(CalendarDtoMapper.ToLookupDto).ToList());
    }

    public virtual async Task<CalendarDto?> GetDefaultAsync(CalendarKind kind)
    {
        await CheckPolicyAsync(CalendarPermissions.Calendars.Default);
        var calendar = await _calendarRepository.FindDefaultAsync(CurrentTenant.Id, kind);
        return calendar == null ? null : CalendarDtoMapper.ToDto(calendar);
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
            input.OwnerType,
            input.OwnerId,
            input.IsDefault,
            input.MaxConcurrent);

        CopyExtraProperties(input.ExtraProperties, calendar.ExtraProperties);
        await _calendarRepository.InsertAsync(calendar, autoSave: true);
        return CalendarDtoMapper.ToDto(calendar);
    }

    public virtual async Task<CalendarDto> UpdateAsync(Guid id, CreateUpdateCalendarDto input)
    {
        await CheckPolicyAsync(CalendarPermissions.Calendars.Update);

        var calendar = await _calendarRepository.GetAsync(id, includeDetails: true);
        calendar.SetName(input.Name);
        calendar.SetKind(input.Kind);
        calendar.SetTimeZone(input.TimeZoneId);
        calendar.SetOwner(input.OwnerType, input.OwnerId);
        await _calendarManager.SetDefaultAsync(calendar, input.IsDefault);
        calendar.SetMaxConcurrent(input.MaxConcurrent);
        CopyExtraProperties(input.ExtraProperties, calendar.ExtraProperties);

        await _calendarRepository.UpdateAsync(calendar, autoSave: true);
        return CalendarDtoMapper.ToDto(calendar);
    }

    public virtual async Task DeleteAsync(Guid id)
    {
        await CheckPolicyAsync(CalendarPermissions.Calendars.Delete);
        await _calendarRepository.DeleteAsync(id, autoSave: true);
    }

    public virtual async Task<ListResultDto<WorkingHourRuleDto>> GetWorkingHoursAsync(Guid calendarId)
    {
        await CheckPolicyAsync(CalendarPermissions.Calendars.Default);
        var calendar = await _calendarRepository.GetAsync(calendarId, includeDetails: true);
        return new ListResultDto<WorkingHourRuleDto>(calendar.WorkingHourRules.Select(CalendarDtoMapper.ToDto).ToList());
    }

    public virtual async Task<ListResultDto<WorkingHourRuleDto>> ReplaceWorkingHoursAsync(Guid calendarId, List<CreateUpdateWorkingHourRuleDto> input)
    {
        await CheckPolicyAsync(CalendarPermissions.Calendars.ManageHours);

        var calendar = await _calendarRepository.GetAsync(calendarId, includeDetails: true);
        var rules = input.Select(x => new WorkingHourRule(GuidGenerator.Create(), calendar.Id, x.DayOfWeek, TimeOnly.FromTimeSpan(x.StartTime), TimeOnly.FromTimeSpan(x.EndTime), x.MaxConcurrent)).ToList();
        calendar.ReplaceWorkingHours(rules);
        await _calendarRepository.UpdateAsync(calendar, autoSave: true);
        return new ListResultDto<WorkingHourRuleDto>(calendar.WorkingHourRules.Select(CalendarDtoMapper.ToDto).ToList());
    }

    public virtual async Task<ListResultDto<CalendarExceptionDto>> GetExceptionsAsync(Guid calendarId)
    {
        await CheckPolicyAsync(CalendarPermissions.Calendars.Default);
        var calendar = await _calendarRepository.GetAsync(calendarId, includeDetails: true);
        return new ListResultDto<CalendarExceptionDto>(calendar.Exceptions.Select(CalendarDtoMapper.ToDto).ToList());
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
            x.Ranges.Select(r => new WorkingHourRange(TimeOnly.FromTimeSpan(r.StartTime), TimeOnly.FromTimeSpan(r.EndTime), r.MaxConcurrent)),
            x.Description)).ToList();

        calendar.ReplaceExceptions(exceptions);
        await _calendarRepository.UpdateAsync(calendar, autoSave: true);
        return new ListResultDto<CalendarExceptionDto>(calendar.Exceptions.Select(CalendarDtoMapper.ToDto).ToList());
    }

    public virtual async Task<TestAvailabilityResultDto> TestAsync(Guid calendarId, TestAvailabilityInput input)
    {
        await CheckPolicyAsync(CalendarPermissions.Calendars.Default);

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

        if (input.OwnerType.HasValue)
        {
            query = query.Where(x => x.OwnerType == input.OwnerType.Value);
        }

        if (input.OwnerId.HasValue)
        {
            query = query.Where(x => x.OwnerId == input.OwnerId.Value);
        }

        return query;
    }

    protected virtual void CopyExtraProperties(IDictionary<string, object?> source, IDictionary<string, object?> target)
    {
        target.Clear();
        foreach (var property in source)
        {
            target[property.Key] = property.Value;
        }
    }
}
