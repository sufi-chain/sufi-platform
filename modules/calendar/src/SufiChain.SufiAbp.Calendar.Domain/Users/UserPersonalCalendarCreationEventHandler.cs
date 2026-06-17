using SufiChain.SufiAbp.Calendar.Calendars;
using SufiChain.SufiAbp.DependencyInjection;
using SufiChain.SufiAbp.EventBus.Distributed;
using SufiChain.SufiAbp.Linq;
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
    protected IAsyncQueryableExecuter AsyncExecuter { get; }

    public UserPersonalCalendarCreationEventHandler(
        ICalendarRepository calendarRepository,
        CalendarManager calendarManager,
        ICurrentTenant currentTenant,
        IGuidGenerator guidGenerator,
        IAsyncQueryableExecuter asyncExecuter)
    {
        CalendarRepository = calendarRepository;
        CalendarManager = calendarManager;
        CurrentTenant = currentTenant;
        GuidGenerator = guidGenerator;
        AsyncExecuter = asyncExecuter;
    }

    [UnitOfWork]
    public virtual async Task HandleEventAsync(EntityCreatedEto<UserEto> eventData)
    {
        using (CurrentTenant.Change(eventData.Entity.TenantId))
        {
            var query = await CalendarRepository.GetQueryableAsync();
            var existingCalendar = await AsyncExecuter.FirstOrDefaultAsync(query.Where(calendar =>
                calendar.Kind == CalendarKind.Personal &&
                calendar.OwnerType == CalendarOwnerType.User &&
                calendar.OwnerId == eventData.Entity.Id));

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
                CalendarOwnerType.User,
                eventData.Entity.Id,
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
