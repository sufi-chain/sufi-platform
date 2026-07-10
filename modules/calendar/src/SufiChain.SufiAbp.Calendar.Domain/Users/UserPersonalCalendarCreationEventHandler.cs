using SufiChain.SufiAbp.Calendar.Calendars;
using SufiChain.SufiAbp.DependencyInjection;
using SufiChain.SufiAbp.EventBus.Distributed;
using SufiChain.SufiAbp.MultiTenancy;
using SufiChain.SufiAbp.Users;
using Volo.Abp.Domain.Entities.Events.Distributed;
using Volo.Abp.Guids;
using Volo.Abp.Uow;

namespace SufiChain.SufiAbp.Calendar.Users;

public class UserPersonalCalendarCreationEventHandler :
    IDistributedEventHandler<EntityCreatedEto<UserEto>>,
    ITransientDependency
{
    protected ICalendarRepository CalendarRepository { get; }
    protected CalendarManager CalendarManager { get; }
    protected ICurrentTenant CurrentTenant { get; }
    protected IGuidGenerator GuidGenerator { get; }

    public UserPersonalCalendarCreationEventHandler(
        ICalendarRepository calendarRepository,
        CalendarManager calendarManager,
        ICurrentTenant currentTenant,
        IGuidGenerator guidGenerator)
    {
        CalendarRepository = calendarRepository;
        CalendarManager = calendarManager;
        CurrentTenant = currentTenant;
        GuidGenerator = guidGenerator;
    }

    [UnitOfWork]
    public virtual async Task HandleEventAsync(EntityCreatedEto<UserEto> eventData)
    {
        using (CurrentTenant.Change(eventData.Entity.TenantId))
        {
            var calendars = await CalendarRepository.GetByOwnerUserIdAsync(eventData.Entity.TenantId, eventData.Entity.Id);
            var existingCalendar = calendars.FirstOrDefault(x => x.Kind == CalendarKind.Personal);

            if (existingCalendar != null)
            {
                return;
            }

            var calendar = await CalendarManager.CreateAsync(
                GuidGenerator.Create(),
                eventData.Entity.TenantId,
                GetCalendarName(eventData.Entity),
                CalendarKind.Personal,
                TimeZoneInfo.Local.Id,
                eventData.Entity.Id,
                eventData.Entity.UserName,
                isDefault: false);

            await CalendarRepository.InsertAsync(calendar, autoSave: true);
        }
    }

    protected virtual string GetCalendarName(UserEto user)
    {
        if (!string.IsNullOrWhiteSpace(user.UserName))
        {
            return user.UserName;
        }

        if (!string.IsNullOrWhiteSpace(user.Email))
        {
            return user.Email;
        }

        return "Personal Calendar";
    }
}
