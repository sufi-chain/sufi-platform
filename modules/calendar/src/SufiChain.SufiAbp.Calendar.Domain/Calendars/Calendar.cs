using SufiChain.SufiAbp.Domain.Entities.Auditing;
using SufiChain.SufiAbp.MultiTenancy;
using SufiChain.SufiAbp;

namespace SufiChain.SufiAbp.Calendar.Calendars;

public class Calendar : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public virtual Guid? TenantId { get; private set; }

    public virtual string Name { get; private set; } = default!;

    public virtual CalendarKind Kind { get; private set; }

    public virtual string TimeZoneId { get; private set; } = default!;

    public virtual CalendarOwnerType OwnerType { get; private set; }

    public virtual Guid? OwnerId { get; private set; }

    public virtual bool IsDefault { get; private set; }

    public virtual int? MaxConcurrent { get; private set; }

    public virtual List<WorkingHourRule> WorkingHourRules { get; private set; } = new();

    public virtual List<CalendarException> Exceptions { get; private set; } = new();

    protected Calendar()
    {
    }

    public Calendar(Guid id, Guid? tenantId, string name, CalendarKind kind, string timeZoneId, CalendarOwnerType ownerType = CalendarOwnerType.None, Guid? ownerId = null, bool isDefault = false, int? maxConcurrent = null)
        : base(id)
    {
        TenantId = tenantId;
        SetName(name);
        SetKind(kind);
        SetTimeZone(timeZoneId);
        SetOwner(ownerType, ownerId);
        SetDefault(isDefault);
        SetMaxConcurrent(maxConcurrent);
    }

    public virtual void SetName(string name)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), CalendarConsts.MaxNameLength);
    }

    public virtual void SetKind(CalendarKind kind)
    {
        if (kind == CalendarKind.Resource && OwnerType is not CalendarOwnerType.None and not CalendarOwnerType.Resource)
        {
            throw new BusinessException(CalendarErrorCodes.InvalidOwner);
        }

        Kind = kind;
    }

    public virtual void SetTimeZone(string timeZoneId)
    {
        Check.NotNullOrWhiteSpace(timeZoneId, nameof(timeZoneId), CalendarConsts.MaxTimeZoneIdLength);
        _ = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        TimeZoneId = timeZoneId;
    }

    public virtual void SetOwner(CalendarOwnerType ownerType, Guid? ownerId)
    {
        if (ownerType == CalendarOwnerType.None && ownerId.HasValue)
        {
            throw new BusinessException(CalendarErrorCodes.InvalidOwner);
        }

        if (ownerType != CalendarOwnerType.None && !ownerId.HasValue)
        {
            throw new BusinessException(CalendarErrorCodes.InvalidOwner);
        }

        if (Kind == CalendarKind.Resource && ownerType != CalendarOwnerType.Resource)
        {
            throw new BusinessException(CalendarErrorCodes.InvalidOwner);
        }

        OwnerType = ownerType;
        OwnerId = ownerId;
    }

    public virtual void SetDefault(bool isDefault)
    {
        IsDefault = isDefault;
    }

    public virtual void SetMaxConcurrent(int? maxConcurrent)
    {
        if (maxConcurrent is <= 0)
        {
            throw new BusinessException(CalendarErrorCodes.InvalidMaxConcurrent);
        }

        MaxConcurrent = maxConcurrent;
    }

    public virtual void ReplaceWorkingHours(IEnumerable<WorkingHourRule> rules)
    {
        var ruleList = rules.ToList();
        foreach (var dayRules in ruleList.GroupBy(x => x.DayOfWeek))
        {
            CalendarException.EnsureNoOverlaps(
                dayRules.Select(x => new WorkingHourRange(x.StartTime, x.EndTime, x.MaxConcurrent)).ToList(),
                CalendarErrorCodes.OverlappingWorkingHours);
        }

        WorkingHourRules.Clear();
        WorkingHourRules.AddRange(ruleList);
        AddDistributedEvent(new SufiChain.SufiAbp.Calendar.Events.CalendarChangedEto(Id, TenantId));
    }

    public virtual void AddWorkingHour(WorkingHourRule rule)
    {
        ReplaceWorkingHours(WorkingHourRules.Concat(new[] { rule }));
    }

    public virtual void RemoveWorkingHour(Guid ruleId)
    {
        WorkingHourRules.RemoveAll(x => x.Id == ruleId);
        AddDistributedEvent(new SufiChain.SufiAbp.Calendar.Events.CalendarChangedEto(Id, TenantId));
    }

    public virtual void ReplaceExceptions(IEnumerable<CalendarException> exceptions)
    {
        Exceptions.Clear();
        Exceptions.AddRange(exceptions);
        AddDistributedEvent(new SufiChain.SufiAbp.Calendar.Events.CalendarChangedEto(Id, TenantId));
    }

    public virtual void AddOrReplaceException(CalendarException exception)
    {
        Exceptions.RemoveAll(x => x.Date == exception.Date);
        Exceptions.Add(exception);
        AddDistributedEvent(new SufiChain.SufiAbp.Calendar.Events.CalendarChangedEto(Id, TenantId));
    }

    public virtual void RemoveException(DateOnly date)
    {
        Exceptions.RemoveAll(x => x.Date == date);
        AddDistributedEvent(new SufiChain.SufiAbp.Calendar.Events.CalendarChangedEto(Id, TenantId));
    }
}
