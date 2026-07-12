using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp;

using Volo.Abp.MultiTenancy;
namespace SufiChain.SufiAbp.Calendar.Calendars;

public class Calendar : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public virtual Guid? TenantId { get; private set; }

    public virtual string Name { get; private set; } = default!;

    public virtual CalendarKind Kind { get; private set; }

    public virtual string TimeZoneId { get; private set; } = default!;

    public virtual Guid? OwnerUserId { get; private set; }

    public virtual string? OwnerName { get; private set; }

    public virtual bool IsDefault { get; private set; }

    public virtual bool IsAlwaysOpen { get; private set; }

    public virtual List<WorkingHourRule> WorkingHourRules { get; private set; } = new();

    public virtual List<CalendarException> Exceptions { get; private set; } = new();

    public virtual List<CalendarInheritance> Inheritances { get; private set; } = new();

    protected Calendar()
    {
    }

    public Calendar(Guid id, Guid? tenantId, string name, CalendarKind kind, string timeZoneId, Guid? ownerUserId = null, string? ownerName = null, bool isDefault = false, bool isAlwaysOpen = false)
        : base(id)
    {
        TenantId = tenantId;
        SetName(name);
        SetKind(kind);
        SetTimeZone(timeZoneId);
        SetOwner(ownerUserId, ownerName);
        SetDefault(isDefault);
        SetAlwaysOpen(isAlwaysOpen);
    }

    public virtual void SetName(string name)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), CalendarConsts.MaxNameLength);
    }

    public virtual void SetKind(CalendarKind kind)
    {
        Kind = kind;
    }

    public virtual void SetTimeZone(string timeZoneId)
    {
        Check.NotNullOrWhiteSpace(timeZoneId, nameof(timeZoneId), CalendarConsts.MaxTimeZoneIdLength);
        _ = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        TimeZoneId = timeZoneId;
    }

    public virtual void SetOwner(Guid? ownerUserId, string? ownerName)
    {
        OwnerName = string.IsNullOrWhiteSpace(ownerName)
            ? null
            : Check.NotNullOrWhiteSpace(ownerName, nameof(ownerName), CalendarConsts.MaxOwnerNameLength);

        OwnerUserId = ownerUserId;
    }

    public virtual void SetDefault(bool isDefault)
    {
        IsDefault = isDefault;
    }

    public virtual void SetAlwaysOpen(bool isAlwaysOpen)
    {
        IsAlwaysOpen = isAlwaysOpen;
    }

    public virtual void ReplaceWorkingHours(IEnumerable<WorkingHourRule> rules)
    {
        var ruleList = rules.ToList();
        foreach (var dayRules in ruleList.GroupBy(x => x.DayOfWeek))
        {
            CalendarException.EnsureNoOverlaps(
                dayRules.Select(x => new WorkingHourRange(x.StartTime, x.EndTime)).ToList(),
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

    public virtual void AddInheritance(CalendarInheritance inheritance)
    {
        Inheritances.Add(inheritance);
        AddDistributedEvent(new SufiChain.SufiAbp.Calendar.Events.CalendarChangedEto(Id, TenantId));
    }

    public virtual void RemoveInheritance(Guid parentCalendarId)
    {
        Inheritances.RemoveAll(x => x.ParentCalendarId == parentCalendarId);
        AddDistributedEvent(new SufiChain.SufiAbp.Calendar.Events.CalendarChangedEto(Id, TenantId));
    }
}
