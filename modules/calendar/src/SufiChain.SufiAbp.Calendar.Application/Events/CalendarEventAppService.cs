using SufiChain.SufiAbp;
using SufiChain.SufiAbp.Application.Dtos;
using SufiChain.SufiAbp.Application.Services;
using SufiChain.SufiAbp.Calendar.Calendars;
using SufiChain.SufiAbp.Calendar.Permissions;
using SufiChain.SufiAbp.Calendar.Scheduling;
using SufiChain.SufiAbp.Linq;

namespace SufiChain.SufiAbp.Calendar.Events;

public class CalendarEventAppService : SufiAbpApplicationService, ICalendarEventAppService
{
    private readonly ICalendarEventRepository _eventRepository;
    private readonly ICalendarEventService _calendarEventService;
    private readonly ICalendarRepository _calendarRepository;
    private readonly IAsyncQueryableExecuter _asyncExecuter;

    public CalendarEventAppService(
        ICalendarEventRepository eventRepository,
        ICalendarEventService calendarEventService,
        ICalendarRepository calendarRepository,
        IAsyncQueryableExecuter asyncExecuter)
    {
        _eventRepository = eventRepository;
        _calendarEventService = calendarEventService;
        _calendarRepository = calendarRepository;
        _asyncExecuter = asyncExecuter;
    }

    public virtual async Task<CalendarEventDto> GetAsync(Guid id)
    {
        await CheckPolicyAsync(CalendarPermissions.Events.Default);
        var calendarEvent = await _eventRepository.GetAsync(id, includeDetails: true);
        await EnsureCanSeeCalendarAsync(calendarEvent.CalendarId);
        return CalendarEventDtoMapper.ToDto(calendarEvent);
    }

    public virtual async Task<PagedResultDto<CalendarEventDto>> GetListAsync(GetEventListInput input)
    {
        await CheckPolicyAsync(CalendarPermissions.Events.Default);

        var query = await _eventRepository.WithDetailsAsync();
        query = await ApplyVisibilityFilterAsync(query);
        query = ApplyFilter(query, input);
        var totalCount = await _asyncExecuter.CountAsync(query);
        var items = await _asyncExecuter.ToListAsync(query.Skip(input.SkipCount).Take(input.MaxResultCount));
        return new PagedResultDto<CalendarEventDto>(totalCount, items.Select(CalendarEventDtoMapper.ToDto).ToList());
    }

    public virtual async Task<ListResultDto<CalendarEventDto>> GetEventsBySourceAsync(string sourceType, string sourceId)
    {
        await CheckPolicyAsync(CalendarPermissions.Events.Default);
        var events = await _eventRepository.GetListBySourceAsync(sourceType, sourceId);
        var visibleCalendarIds = await GetVisibleCalendarIdsAsync();
        return new ListResultDto<CalendarEventDto>(events
            .Where(x => visibleCalendarIds.Contains(x.CalendarId))
            .Select(CalendarEventDtoMapper.ToDto)
            .ToList());
    }

    public virtual async Task<CalendarEventDto> CreateAsync(CreateUpdateCalendarEventDto input)
    {
        await CheckPolicyAsync(CalendarPermissions.Events.Create);
        await EnsureCanSeeCalendarAsync(input.CalendarId);

        var calendarEvent = new CalendarEvent(
            GuidGenerator.Create(),
            CurrentTenant.Id,
            input.CalendarId,
            input.Title,
            input.StartUtc,
            input.EndUtc,
            input.IsAllDay,
            input.TimeZoneId,
            input.Status,
            input.AvailabilityCalendarId,
            input.Location,
            input.Description,
            input.Color,
            input.SourceType,
            input.SourceId);

        CopyExtraProperties(input.ExtraProperties, calendarEvent.ExtraProperties);
        if (!string.IsNullOrWhiteSpace(input.RecurrenceRule))
        {
            calendarEvent.SetRecurrence(GuidGenerator.Create(), input.RecurrenceRule);
        }

        await _eventRepository.InsertAsync(calendarEvent, autoSave: true);
        return CalendarEventDtoMapper.ToDto(calendarEvent);
    }

    public virtual async Task<CalendarEventDto> UpdateAsync(Guid id, CreateUpdateCalendarEventDto input)
    {
        await CheckPolicyAsync(CalendarPermissions.Events.Update);

        var calendarEvent = await _eventRepository.GetAsync(id, includeDetails: true);
        await EnsureCanSeeCalendarAsync(calendarEvent.CalendarId);
        await EnsureCanSeeCalendarAsync(input.CalendarId);
        calendarEvent.SetTitle(input.Title);
        calendarEvent.SetTimeRange(input.StartUtc, input.EndUtc, input.IsAllDay, input.TimeZoneId);
        calendarEvent.SetStatus(input.Status);
        calendarEvent.SetAvailabilityCalendar(input.AvailabilityCalendarId);
        calendarEvent.SetDetails(input.Location, input.Description, input.Color);
        calendarEvent.AttachSource(input.SourceType, input.SourceId);
        CopyExtraProperties(input.ExtraProperties, calendarEvent.ExtraProperties);

        if (string.IsNullOrWhiteSpace(input.RecurrenceRule))
        {
            calendarEvent.ClearRecurrence();
        }
        else
        {
            calendarEvent.SetRecurrence(calendarEvent.RecurrenceRule?.Id ?? GuidGenerator.Create(), input.RecurrenceRule);
        }

        await _eventRepository.UpdateAsync(calendarEvent, autoSave: true);
        return CalendarEventDtoMapper.ToDto(calendarEvent);
    }

    public virtual async Task DeleteAsync(Guid id)
    {
        await CheckPolicyAsync(CalendarPermissions.Events.Delete);
        var calendarEvent = await _eventRepository.GetAsync(id);
        await EnsureCanSeeCalendarAsync(calendarEvent.CalendarId);
        await _eventRepository.DeleteAsync(calendarEvent, autoSave: true);
    }

    public virtual async Task<ListResultDto<EventOccurrenceDto>> GetOccurrencesAsync(Guid calendarId, GetOccurrencesInput input)
    {
        await CheckPolicyAsync(CalendarPermissions.Events.Default);
        await EnsureCanSeeCalendarAsync(calendarId);
        var occurrences = await _calendarEventService.ExpandAsync(calendarId, input.FromUtc, input.ToUtc);
        return new ListResultDto<EventOccurrenceDto>(occurrences.Select(CalendarEventDtoMapper.ToDto).ToList());
    }

    public virtual async Task<CalendarEventDto> SetRecurrenceAsync(Guid id, string recurrenceRule)
    {
        await CheckPolicyAsync(CalendarPermissions.Events.Update);
        var calendarEvent = await _eventRepository.GetAsync(id, includeDetails: true);
        await EnsureCanSeeCalendarAsync(calendarEvent.CalendarId);
        calendarEvent.SetRecurrence(calendarEvent.RecurrenceRule?.Id ?? GuidGenerator.Create(), recurrenceRule);
        await _eventRepository.UpdateAsync(calendarEvent, autoSave: true);
        return CalendarEventDtoMapper.ToDto(calendarEvent);
    }

    public virtual async Task<CalendarEventDto> ClearRecurrenceAsync(Guid id)
    {
        await CheckPolicyAsync(CalendarPermissions.Events.Update);
        var calendarEvent = await _eventRepository.GetAsync(id, includeDetails: true);
        await EnsureCanSeeCalendarAsync(calendarEvent.CalendarId);
        calendarEvent.ClearRecurrence();
        await _eventRepository.UpdateAsync(calendarEvent, autoSave: true);
        return CalendarEventDtoMapper.ToDto(calendarEvent);
    }

    public virtual async Task<CalendarEventDto> MoveOccurrenceAsync(Guid id, MoveOccurrenceDto input)
    {
        await CheckPolicyAsync(CalendarPermissions.Events.Update);
        var calendarEvent = await _eventRepository.GetAsync(id, includeDetails: true);
        await EnsureCanSeeCalendarAsync(calendarEvent.CalendarId);
        calendarEvent.MoveOccurrence(GuidGenerator.Create(), input.OriginalStartUtc, input.MovedStartUtc, input.MovedEndUtc, input.ThisAndFollowing);
        await _eventRepository.UpdateAsync(calendarEvent, autoSave: true);
        return CalendarEventDtoMapper.ToDto(calendarEvent);
    }

    public virtual async Task<CalendarEventDto> CancelOccurrenceAsync(Guid id, CancelOccurrenceDto input)
    {
        await CheckPolicyAsync(CalendarPermissions.Events.Update);
        var calendarEvent = await _eventRepository.GetAsync(id, includeDetails: true);
        await EnsureCanSeeCalendarAsync(calendarEvent.CalendarId);
        calendarEvent.CancelOccurrence(GuidGenerator.Create(), input.OriginalStartUtc, input.ThisAndFollowing);
        await _eventRepository.UpdateAsync(calendarEvent, autoSave: true);
        return CalendarEventDtoMapper.ToDto(calendarEvent);
    }

    public virtual async Task<CalendarEventDto> AddAttendeeAsync(Guid id, CreateEventAttendeeDto input)
    {
        await CheckPolicyAsync(CalendarPermissions.Events.ManageAttendees);
        var calendarEvent = await _eventRepository.GetAsync(id, includeDetails: true);
        await EnsureCanSeeCalendarAsync(calendarEvent.CalendarId);
        calendarEvent.AddAttendee(new EventAttendee(GuidGenerator.Create(), id, input.UserId, input.Email, input.DisplayName, input.Role));
        await _eventRepository.UpdateAsync(calendarEvent, autoSave: true);
        return CalendarEventDtoMapper.ToDto(calendarEvent);
    }

    public virtual async Task<CalendarEventDto> RemoveAttendeeAsync(Guid id, Guid attendeeId)
    {
        await CheckPolicyAsync(CalendarPermissions.Events.ManageAttendees);
        var calendarEvent = await _eventRepository.GetAsync(id, includeDetails: true);
        await EnsureCanSeeCalendarAsync(calendarEvent.CalendarId);
        calendarEvent.RemoveAttendee(attendeeId);
        await _eventRepository.UpdateAsync(calendarEvent, autoSave: true);
        return CalendarEventDtoMapper.ToDto(calendarEvent);
    }

    public virtual async Task<CalendarEventDto> SetRsvpAsync(Guid id, Guid attendeeId, RsvpStatus rsvpStatus)
    {
        await CheckPolicyAsync(CalendarPermissions.Events.Default);
        var calendarEvent = await _eventRepository.GetAsync(id, includeDetails: true);
        await EnsureCanSeeCalendarAsync(calendarEvent.CalendarId);
        calendarEvent.SetRsvp(attendeeId, rsvpStatus);
        await _eventRepository.UpdateAsync(calendarEvent, autoSave: true);
        return CalendarEventDtoMapper.ToDto(calendarEvent);
    }

    public virtual async Task<CalendarEventDto> AddReminderAsync(Guid id, CreateEventReminderDto input)
    {
        await CheckPolicyAsync(CalendarPermissions.Events.Update);
        var calendarEvent = await _eventRepository.GetAsync(id, includeDetails: true);
        await EnsureCanSeeCalendarAsync(calendarEvent.CalendarId);
        calendarEvent.AddReminder(new EventReminder(GuidGenerator.Create(), id, input.Offset, input.Channel, input.AttendeeId));
        await _eventRepository.UpdateAsync(calendarEvent, autoSave: true);
        return CalendarEventDtoMapper.ToDto(calendarEvent);
    }

    public virtual async Task<CalendarEventDto> RemoveReminderAsync(Guid id, Guid reminderId)
    {
        await CheckPolicyAsync(CalendarPermissions.Events.Update);
        var calendarEvent = await _eventRepository.GetAsync(id, includeDetails: true);
        await EnsureCanSeeCalendarAsync(calendarEvent.CalendarId);
        calendarEvent.RemoveReminder(reminderId);
        await _eventRepository.UpdateAsync(calendarEvent, autoSave: true);
        return CalendarEventDtoMapper.ToDto(calendarEvent);
    }

    protected virtual IQueryable<CalendarEvent> ApplyFilter(IQueryable<CalendarEvent> query, GetEventListInput input)
    {
        if (input.CalendarId.HasValue)
        {
            query = query.Where(x => x.CalendarId == input.CalendarId.Value);
        }

        if (input.FromUtc.HasValue)
        {
            query = query.Where(x => x.RecurrenceRule != null || x.EndUtc > input.FromUtc.Value);
        }

        if (input.ToUtc.HasValue)
        {
            query = query.Where(x => x.RecurrenceRule != null || x.StartUtc < input.ToUtc.Value);
        }

        if (!string.IsNullOrWhiteSpace(input.SourceType))
        {
            query = query.Where(x => x.SourceType == input.SourceType);
        }

        if (!string.IsNullOrWhiteSpace(input.SourceId))
        {
            query = query.Where(x => x.SourceId == input.SourceId);
        }

        return query;
    }

    protected virtual async Task<IQueryable<CalendarEvent>> ApplyVisibilityFilterAsync(IQueryable<CalendarEvent> query)
    {
        var visibleCalendarIds = await GetVisibleCalendarIdsAsync();
        return query.Where(x => visibleCalendarIds.Contains(x.CalendarId));
    }

    protected virtual async Task EnsureCanSeeCalendarAsync(Guid calendarId)
    {
        var query = await _calendarRepository.GetQueryableAsync();
        var canSee = await _asyncExecuter.AnyAsync(GetVisibleCalendarQuery(query).Where(x => x.Id == calendarId));
        if (!canSee)
        {
            throw new BusinessException(CalendarErrorCodes.InvalidOwner);
        }
    }

    protected virtual async Task<List<Guid>> GetVisibleCalendarIdsAsync()
    {
        var query = await _calendarRepository.GetQueryableAsync();
        return await _asyncExecuter.ToListAsync(GetVisibleCalendarQuery(query).Select(x => x.Id));
    }

    protected virtual IQueryable<Calendars.Calendar> GetVisibleCalendarQuery(IQueryable<Calendars.Calendar> query)
    {
        var userId = CurrentUser.Id;

        return query.Where(x =>
            x.OwnerType == CalendarOwnerType.None ||
            x.Kind == CalendarKind.Tenant ||
            (userId.HasValue && x.OwnerType == CalendarOwnerType.User && x.OwnerId == userId.Value));
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
