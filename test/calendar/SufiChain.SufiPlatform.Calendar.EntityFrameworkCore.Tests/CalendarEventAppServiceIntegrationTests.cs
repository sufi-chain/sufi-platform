using Shouldly;
using SufiChain.SufiPlatform.Calendar.Calendars;
using SufiChain.SufiPlatform.Calendar.Events;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.Security.Claims;
using System.Security.Claims;
using Xunit;

namespace SufiChain.SufiPlatform.Calendar.EntityFrameworkCore;

public class CalendarEventAppServiceIntegrationTests : CalendarEntityFrameworkCoreTestBase
{
    private static readonly DateTime StartUtc = new(2026, 9, 8, 10, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Should_Hide_Private_Calendar_From_Another_User_Despite_Granted_Permissions()
    {
        var ownerId = Guid.NewGuid();
        var calendarId = Guid.NewGuid();
        await WithUnitOfWorkAsync(async () =>
            await GetRequiredService<ICalendarRepository>().InsertAsync(
                new Calendars.Calendar(calendarId, null, "Private", CalendarKind.Personal, "UTC", ownerId), autoSave: true));

        var accessor = GetRequiredService<ICurrentPrincipalAccessor>();
        var service = GetRequiredService<ICalendarEventAppService>();
        Guid eventId;
        using (accessor.Change(Principal(ownerId)))
        {
            eventId = (await service.CreateAsync(NewEvent(calendarId))).Id;
            (await service.GetAsync(eventId)).CalendarId.ShouldBe(calendarId);
        }

        using (accessor.Change(Principal(Guid.NewGuid())))
        {
            (await service.GetListAsync(new GetEventListInput())).Items.ShouldBeEmpty();
            var readError = await Should.ThrowAsync<BusinessException>(() => service.GetAsync(eventId));
            readError.Code.ShouldBe(CalendarErrorCodes.CalendarNotAccessible);
            var writeError = await Should.ThrowAsync<BusinessException>(() => service.CreateAsync(NewEvent(calendarId)));
            writeError.Code.ShouldBe(CalendarErrorCodes.CalendarNotAccessible);
        }

        using (accessor.Change(Principal(ownerId)))
        {
            (await service.GetListAsync(new GetEventListInput())).TotalCount.ShouldBe(1);
        }
    }

    private static ClaimsPrincipal Principal(Guid id) => new(new ClaimsIdentity(
        new[] { new Claim(AbpClaimTypes.UserId, id.ToString()) }, "CalendarTests"));

    [Fact]
    public async Task Should_Create_Update_And_Soft_Delete_Event_Through_Application_Service()
    {
        var calendarId = await CreateCalendarAsync();
        var service = GetRequiredService<ICalendarEventAppService>();
        var input = NewEvent(calendarId);
        var created = await service.CreateAsync(input);
        created.Id.ShouldNotBe(Guid.Empty);

        // Read through a new unit of work so assertions exercise database round trips.
        await WithUnitOfWorkAsync(async () =>
        {
            var saved = await GetRequiredService<ICalendarEventRepository>().GetAsync(created.Id);
            saved.Title.ShouldBe(input.Title);
            saved.StartUtc.ShouldBe(StartUtc);
            saved.CalendarId.ShouldBe(calendarId);
        });

        input.Title = "Rescheduled planning";
        input.StartUtc = StartUtc.AddHours(2);
        input.EndUtc = StartUtc.AddHours(3);
        await service.UpdateAsync(created.Id, input);
        var updated = await service.GetAsync(created.Id);
        updated.Title.ShouldBe(input.Title);
        updated.StartUtc.ShouldBe(input.StartUtc);
        updated.EndUtc.ShouldBe(input.EndUtc);

        await service.DeleteAsync(created.Id);
        (await service.GetListAsync(new GetEventListInput { CalendarId = calendarId })).Items.ShouldBeEmpty();
        await WithUnitOfWorkAsync(async () =>
            (await GetRequiredService<ICalendarEventRepository>().FindAsync(created.Id)).ShouldBeNull());
        await WithUnitOfWorkAsync(async () =>
        {
            using (GetRequiredService<IDataFilter>().Disable<ISoftDelete>())
            {
                var deleted = await GetRequiredService<ICalendarEventRepository>().GetAsync(created.Id);
                deleted.IsDeleted.ShouldBeTrue();
                deleted.DeletionTime.ShouldNotBeNull();
            }
        });
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Should_Reject_Nonpositive_Duration_Without_Inserting_Event(int hours)
    {
        var calendarId = await CreateCalendarAsync();
        var input = NewEvent(calendarId);
        input.EndUtc = StartUtc.AddHours(hours);
        var error = await Should.ThrowAsync<BusinessException>(() =>
            GetRequiredService<ICalendarEventAppService>().CreateAsync(input));
        error.Code.ShouldBe(CalendarErrorCodes.InvalidTimeRange);
        await WithUnitOfWorkAsync(async () =>
            (await GetRequiredService<ICalendarEventRepository>().GetCountAsync()).ShouldBe(0));
    }

    [Fact]
    public async Task Should_Hide_Another_Tenants_Events_And_Reject_Create_In_Its_Calendar()
    {
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        Guid calendarA;
        using (CurrentTenant.Change(tenantA))
        {
            calendarA = await CreateCalendarAsync();
            await GetRequiredService<ICalendarEventAppService>().CreateAsync(NewEvent(calendarA));
        }

        using (CurrentTenant.Change(tenantB))
        {
            var calendarB = await CreateCalendarAsync();
            var service = GetRequiredService<ICalendarEventAppService>();
            var ownEvent = await service.CreateAsync(NewEvent(calendarB));
            var list = await service.GetListAsync(new GetEventListInput());
            list.TotalCount.ShouldBe(1);
            list.Items.Single().Id.ShouldBe(ownEvent.Id);
            var error = await Should.ThrowAsync<BusinessException>(() => service.CreateAsync(NewEvent(calendarA)));
            error.Code.ShouldBe(CalendarErrorCodes.CalendarNotAccessible);
            await WithUnitOfWorkAsync(async () =>
                (await GetRequiredService<ICalendarEventRepository>().GetCountAsync()).ShouldBe(1));
        }

        using (CurrentTenant.Change(tenantA))
        {
            var list = await GetRequiredService<ICalendarEventAppService>().GetListAsync(new GetEventListInput());
            list.TotalCount.ShouldBe(1);
            list.Items.Single().CalendarId.ShouldBe(calendarA);
        }
    }

    private async Task<Guid> CreateCalendarAsync()
    {
        var id = Guid.NewGuid();
        await WithUnitOfWorkAsync(async () =>
            await GetRequiredService<ICalendarRepository>().InsertAsync(
                new Calendars.Calendar(id, CurrentTenant.Id, "Team calendar", CalendarKind.Public, "UTC"), autoSave: true));
        return id;
    }

    private static CreateUpdateCalendarEventDto NewEvent(Guid calendarId) => new()
    {
        CalendarId = calendarId,
        Title = "Planning",
        StartUtc = StartUtc,
        EndUtc = StartUtc.AddHours(1),
        TimeZoneId = "UTC"
    };
}
