using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Shouldly;
using SufiChain.SufiPlatform.Calendar.Calendars;
using SufiChain.SufiPlatform.Calendar.Events;
using SufiChain.SufiPlatform.Calendar.Permissions;
using Volo.Abp;
using Volo.Abp.Security.Claims;
using Xunit;

namespace SufiChain.SufiPlatform.Calendar.EntityFrameworkCore;

public class CalendarShareTests : CalendarEntityFrameworkCoreTestBase
{
    private static readonly DateTime StartUtc = new(2026, 9, 8, 10, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task CalendarShare_Grant_ReadOnlyUserCanViewCannotEdit()
    {
        var ownerId = Guid.NewGuid();
        var granteeId = Guid.NewGuid();
        var calendarId = await CreatePersonalCalendarAsync(ownerId);
        var accessor = GetRequiredService<ICurrentPrincipalAccessor>();
        var events = GetRequiredService<ICalendarEventAppService>();
        Guid eventId;
        using (accessor.Change(Principal(ownerId)))
        {
            eventId = (await events.CreateAsync(NewEvent(calendarId))).Id;
        }

        var granted = await GetRequiredService<ICalendarShareAppService>().GrantAsync(
            calendarId,
            new GrantCalendarShareInput { UserId = granteeId });
        granted.UserId.ShouldBe(granteeId);

        using (accessor.Change(Principal(granteeId)))
        {
            (await events.GetAsync(eventId)).Title.ShouldBe("Planning");
            var error = await Should.ThrowAsync<BusinessException>(() => events.UpdateAsync(eventId, NewEvent(calendarId)));
            error.Code.ShouldBe(CalendarErrorCodes.CalendarShareReadOnly);
        }
    }

    [Fact]
    public async Task CalendarShare_Revoke_RemovesAccess()
    {
        var ownerId = Guid.NewGuid();
        var granteeId = Guid.NewGuid();
        var calendarId = await CreatePersonalCalendarAsync(ownerId);
        var shares = GetRequiredService<ICalendarShareAppService>();
        var granted = await shares.GrantAsync(calendarId, new GrantCalendarShareInput { UserId = granteeId });
        var accessor = GetRequiredService<ICurrentPrincipalAccessor>();
        var events = GetRequiredService<ICalendarEventAppService>();
        Guid eventId;
        using (accessor.Change(Principal(ownerId)))
        {
            eventId = (await events.CreateAsync(NewEvent(calendarId))).Id;
        }

        await shares.RevokeAsync(calendarId, granted.Id);

        using (accessor.Change(Principal(granteeId)))
        {
            var error = await Should.ThrowAsync<BusinessException>(() => events.GetAsync(eventId));
            error.Code.ShouldBe(CalendarErrorCodes.CalendarNotAccessible);
        }
    }

    [Fact]
    public void CalendarShare_RequiresSharePermission()
    {
        var authorize = typeof(CalendarShareAppService).GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .Cast<AuthorizeAttribute>()
            .Single();
        authorize.Policy.ShouldBe(CalendarPermissions.Calendars.Share);
    }

    [Fact]
    public void CalendarShare_Api_IsUnauthorizedWithoutShare()
    {
        var source = Read("modules/calendar/src/SufiChain.SufiPlatform.Calendar.HttpApi/Controllers/CalendarShareController.cs");
        source.ShouldContain("[Authorize(CalendarPermissions.Calendars.Share)]");
        source.ShouldContain("[Route(\"{calendarId}/shares\")]");
    }

    private static string Read(string relative)
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory != null; directory = directory.Parent)
        {
            var candidate = Path.Combine(directory.FullName, relative);
            if (File.Exists(candidate))
            {
                return File.ReadAllText(candidate);
            }
        }

        throw new FileNotFoundException(relative);
    }

    private async Task<Guid> CreatePersonalCalendarAsync(Guid ownerId)
    {
        var id = Guid.NewGuid();
        await WithUnitOfWorkAsync(async () =>
            await GetRequiredService<ICalendarRepository>().InsertAsync(
                new Calendars.Calendar(id, CurrentTenant.Id, "Personal", CalendarKind.Personal, "UTC", ownerId),
                autoSave: true));
        return id;
    }

    private static ClaimsPrincipal Principal(Guid id) => new(new ClaimsIdentity(
        [new Claim(AbpClaimTypes.UserId, id.ToString())],
        "CalendarTests"));

    private static CreateUpdateCalendarEventDto NewEvent(Guid calendarId) => new()
    {
        CalendarId = calendarId,
        Title = "Planning",
        StartUtc = StartUtc,
        EndUtc = StartUtc.AddHours(1),
        TimeZoneId = "UTC"
    };
}
