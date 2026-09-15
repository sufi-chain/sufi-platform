using System.Security.Claims;
using Shouldly;
using SufiChain.SufiPlatform.Calendar.Calendars;
using SufiChain.SufiPlatform.Calendar.Events;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.Security.Claims;
using Xunit;

namespace SufiChain.SufiPlatform.Calendar.MongoDB;

public class CalendarMongoDBBusinessTests : CalendarMongoDBTestBase
{
    private static readonly DateTime StartUtc = new(2026, 9, 8, 10, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Should_Persist_Default_Inheritance_And_Enforce_Default_Uniqueness_Per_Tenant()
    {
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        using (CurrentTenant.Change(tenantA))
        {
            var defaultId = await CreateDefaultAsync();
            var error = await Should.ThrowAsync<BusinessException>(() => CreateDefaultAsync());
            error.Code.ShouldBe(CalendarErrorCodes.DefaultCalendarAlreadyExists);
            var personalId = Guid.NewGuid();
            await WithUnitOfWorkAsync(async () =>
            {
                var personal = await GetRequiredService<CalendarManager>().CreateAsync(personalId, tenantA,
                    "Personal", CalendarKind.Personal, "UTC");
                await GetRequiredService<ICalendarRepository>().InsertAsync(personal, autoSave: true);
            });
            await WithUnitOfWorkAsync(async () =>
            {
                var repository = GetRequiredService<ICalendarRepository>();
                var personal = await repository.GetAsync(personalId);
                personal.Inheritances.Single().ParentCalendarId.ShouldBe(defaultId);
                personal.Inheritances.Single().IsInheritedByDefault.ShouldBeFalse();
                (await repository.GetInheritingCalendarIdsAsync(defaultId)).ShouldContain(personalId);
                (await repository.GetCountAsync()).ShouldBe(2);
            });
        }

        using (CurrentTenant.Change(tenantB))
        {
            var defaultId = await CreateDefaultAsync();
            await WithUnitOfWorkAsync(async () =>
            {
                var repository = GetRequiredService<ICalendarRepository>();
                (await repository.GetCountAsync()).ShouldBe(1);
                (await repository.FindDefaultAsync(tenantB, CalendarKind.Default))!.Id.ShouldBe(defaultId);
                (await repository.FindDefaultAsync(tenantA, CalendarKind.Default)).ShouldBeNull();
            });
        }
    }

    [Fact]
    public async Task Should_Persist_Application_Updates_And_Soft_Delete()
    {
        var calendarId = await CreateCalendarAsync();
        var service = GetRequiredService<ICalendarEventAppService>();
        var input = NewEvent(calendarId);
        var created = await service.CreateAsync(input);
        await WithUnitOfWorkAsync(async () =>
            (await GetRequiredService<ICalendarEventRepository>().GetAsync(created.Id)).Title.ShouldBe("Planning"));

        input.Title = "Rescheduled";
        input.StartUtc = StartUtc.AddHours(2);
        input.EndUtc = StartUtc.AddHours(3);
        await service.UpdateAsync(created.Id, input);
        var saved = await service.GetAsync(created.Id);
        saved.Title.ShouldBe("Rescheduled");
        saved.StartUtc.ShouldBe(input.StartUtc);
        saved.EndUtc.ShouldBe(input.EndUtc);

        await service.DeleteAsync(created.Id);
        (await service.GetListAsync(new GetEventListInput { CalendarId = calendarId })).Items.ShouldBeEmpty();
        await WithUnitOfWorkAsync(async () =>
        {
            var repository = GetRequiredService<ICalendarEventRepository>();
            (await repository.FindAsync(created.Id)).ShouldBeNull();
            using (GetRequiredService<IDataFilter>().Disable<ISoftDelete>())
            {
                var deleted = await repository.GetAsync(created.Id);
                deleted.IsDeleted.ShouldBeTrue();
                deleted.DeletionTime.ShouldNotBeNull();
            }
        });
    }

    [Fact]
    public async Task Should_Hide_Private_Events_From_Other_Users()
    {
        var owner = Guid.NewGuid();
        var calendarId = await CreateCalendarAsync(owner);
        var service = GetRequiredService<ICalendarEventAppService>();
        var accessor = GetRequiredService<ICurrentPrincipalAccessor>();
        Guid eventId;
        using (accessor.Change(Principal(owner)))
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
    }

    [Fact]
    public async Task Should_Isolate_Events_And_Reject_A_Foreign_Tenant_Calendar()
    {
        Guid calendarA;
        using (CurrentTenant.Change(Guid.NewGuid()))
        {
            calendarA = await CreateCalendarAsync();
            await GetRequiredService<ICalendarEventAppService>().CreateAsync(NewEvent(calendarA));
        }
        using (CurrentTenant.Change(Guid.NewGuid()))
        {
            var calendarB = await CreateCalendarAsync();
            var service = GetRequiredService<ICalendarEventAppService>();
            var own = await service.CreateAsync(NewEvent(calendarB));
            var list = await service.GetListAsync(new GetEventListInput());
            list.TotalCount.ShouldBe(1);
            list.Items.Single().Id.ShouldBe(own.Id);
            var error = await Should.ThrowAsync<BusinessException>(() => service.CreateAsync(NewEvent(calendarA)));
            error.Code.ShouldBe(CalendarErrorCodes.CalendarNotAccessible);
            await WithUnitOfWorkAsync(async () =>
                (await GetRequiredService<ICalendarEventRepository>().GetCountAsync()).ShouldBe(1));
        }
    }

    [Fact]
    public async Task Should_Replace_Embedded_Occurrence_Exception_And_Clear_Recurrence()
    {
        var calendarId = await CreateCalendarAsync();
        var service = GetRequiredService<ICalendarEventAppService>();
        var input = NewEvent(calendarId);
        input.RecurrenceRule = "FREQ=DAILY;COUNT=3";
        var created = await service.CreateAsync(input);
        var original = StartUtc.AddDays(1);
        await service.MoveOccurrenceAsync(created.Id, new MoveOccurrenceDto
        {
            OriginalStartUtc = original,
            MovedStartUtc = original.AddHours(2),
            MovedEndUtc = original.AddHours(3)
        });
        await WithUnitOfWorkAsync(async () =>
        {
            var saved = await GetRequiredService<ICalendarEventRepository>().GetAsync(created.Id);
            saved.RecurrenceRule.ShouldNotBeNull();
            saved.RecurrenceRule.Count.ShouldBe(3);
            saved.OccurrenceExceptions.Single().OverrideStartUtc.ShouldBe(original.AddHours(2));
        });

        await service.CancelOccurrenceAsync(created.Id, new CancelOccurrenceDto { OriginalStartUtc = original });
        await WithUnitOfWorkAsync(async () =>
        {
            var saved = await GetRequiredService<ICalendarEventRepository>().GetAsync(created.Id);
            var exception = saved.OccurrenceExceptions.Single();
            exception.OriginalStartUtc.ShouldBe(original);
            exception.IsCancelled.ShouldBeTrue();
            exception.OverrideStartUtc.ShouldBeNull();
        });
        await service.ClearRecurrenceAsync(created.Id);
        await WithUnitOfWorkAsync(async () =>
        {
            var saved = await GetRequiredService<ICalendarEventRepository>().GetAsync(created.Id);
            saved.RecurrenceRule.ShouldBeNull();
            saved.OccurrenceExceptions.ShouldBeEmpty();
        });
    }

    [Fact]
    public async Task Should_Persist_Embedded_Attendee_Rsvp_And_Reminder_Mutations()
    {
        var calendarId = await CreateCalendarAsync();
        var service = GetRequiredService<ICalendarEventAppService>();
        var created = await service.CreateAsync(NewEvent(calendarId));
        var withAttendee = await service.AddAttendeeAsync(created.Id, new CreateEventAttendeeDto
        {
            Email = "participant@example.test", DisplayName = "Participant", Role = AttendeeRole.Required
        });
        var attendeeId = withAttendee.Attendees.Single().Id;
        await service.SetRsvpAsync(created.Id, attendeeId, RsvpStatus.Accepted);
        var withReminder = await service.AddReminderAsync(created.Id, new CreateEventReminderDto
        {
            AttendeeId = attendeeId, Channel = ReminderChannel.Email, Offset = TimeSpan.FromMinutes(-15)
        });
        var reminderId = withReminder.Reminders.Single().Id;
        await WithUnitOfWorkAsync(async () =>
        {
            var saved = await GetRequiredService<ICalendarEventRepository>().GetAsync(created.Id);
            saved.Attendees.Single().RsvpStatus.ShouldBe(RsvpStatus.Accepted);
            saved.Attendees.Single().Email.ShouldBe("participant@example.test");
            saved.Reminders.Single().AttendeeId.ShouldBe(attendeeId);
            saved.Reminders.Single().Offset.ShouldBe(TimeSpan.FromMinutes(-15));
        });
        await service.RemoveReminderAsync(created.Id, reminderId);
        await service.RemoveAttendeeAsync(created.Id, attendeeId);
        await WithUnitOfWorkAsync(async () =>
        {
            var saved = await GetRequiredService<ICalendarEventRepository>().GetAsync(created.Id);
            saved.Attendees.ShouldBeEmpty();
            saved.Reminders.ShouldBeEmpty();
        });
    }

    private async Task<Guid> CreateDefaultAsync()
    {
        var id = Guid.NewGuid();
        await WithUnitOfWorkAsync(async () =>
        {
            var calendar = await GetRequiredService<CalendarManager>().CreateAsync(id, CurrentTenant.Id,
                "Default", CalendarKind.Default, "UTC", isDefault: true);
            await GetRequiredService<ICalendarRepository>().InsertAsync(calendar, autoSave: true);
        });
        return id;
    }

    private async Task<Guid> CreateCalendarAsync(Guid? ownerId = null)
    {
        var id = Guid.NewGuid();
        await WithUnitOfWorkAsync(async () =>
            await GetRequiredService<ICalendarRepository>().InsertAsync(new Calendars.Calendar(id, CurrentTenant.Id,
                "Team", ownerId.HasValue ? CalendarKind.Personal : CalendarKind.Public, "UTC", ownerId), autoSave: true));
        return id;
    }

    private static CreateUpdateCalendarEventDto NewEvent(Guid calendarId) => new()
    {
        CalendarId = calendarId, Title = "Planning", StartUtc = StartUtc,
        EndUtc = StartUtc.AddHours(1), TimeZoneId = "UTC"
    };

    private static ClaimsPrincipal Principal(Guid id) => new(new ClaimsIdentity(
        new[] { new Claim(AbpClaimTypes.UserId, id.ToString()) }, "CalendarMongoDBTests"));
}
