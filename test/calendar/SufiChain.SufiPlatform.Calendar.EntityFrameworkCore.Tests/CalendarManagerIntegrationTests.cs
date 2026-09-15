using Shouldly;
using SufiChain.SufiPlatform.Calendar.Calendars;
using Volo.Abp;
using Xunit;

namespace SufiChain.SufiPlatform.Calendar.EntityFrameworkCore;

public class CalendarManagerIntegrationTests : CalendarEntityFrameworkCoreTestBase
{
    [Fact]
    public async Task Should_Persist_Default_Inheritance_Without_Inheriting_Working_Hours()
    {
        var tenantId = Guid.NewGuid();
        var defaultId = Guid.NewGuid();
        var personalId = Guid.NewGuid();
        using (CurrentTenant.Change(tenantId))
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var manager = GetRequiredService<CalendarManager>();
                var repository = GetRequiredService<ICalendarRepository>();
                await repository.InsertAsync(await manager.CreateAsync(defaultId, tenantId,
                    "Tenant default", CalendarKind.Default, "UTC", isDefault: true), autoSave: true);
                await repository.InsertAsync(await manager.CreateAsync(personalId, tenantId,
                    "Personal", CalendarKind.Personal, "UTC"), autoSave: true);
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var calendar = await GetRequiredService<ICalendarRepository>().GetAsync(personalId, includeDetails: true);
                calendar.Inheritances.Count.ShouldBe(1);
                calendar.Inheritances[0].ParentCalendarId.ShouldBe(defaultId);
                calendar.Inheritances[0].IsInheritedByDefault.ShouldBeFalse();
            });
        }
    }

    [Fact]
    public async Task Should_Reject_Second_Default_In_Same_Tenant_But_Allow_Another_Tenant()
    {
        var tenantId = Guid.NewGuid();
        using (CurrentTenant.Change(tenantId))
        {
            await CreateDefaultAsync(tenantId);
            var error = await Should.ThrowAsync<BusinessException>(() => CreateDefaultAsync(tenantId));
            error.Code.ShouldBe(CalendarErrorCodes.DefaultCalendarAlreadyExists);
            await WithUnitOfWorkAsync(async () =>
                (await GetRequiredService<ICalendarRepository>().GetCountAsync()).ShouldBe(1));
        }

        var otherTenantId = Guid.NewGuid();
        using (CurrentTenant.Change(otherTenantId))
        {
            await CreateDefaultAsync(otherTenantId);
            await WithUnitOfWorkAsync(async () =>
            {
                var repository = GetRequiredService<ICalendarRepository>();
                (await repository.GetCountAsync()).ShouldBe(1);
                var calendar = await repository.FindDefaultAsync(otherTenantId, CalendarKind.Default);
                calendar.ShouldNotBeNull();
                calendar.TenantId.ShouldBe(otherTenantId);
                (await repository.FindDefaultAsync(tenantId, CalendarKind.Default)).ShouldBeNull();
            });
        }
    }

    [Fact]
    public async Task Should_Reject_Self_Inheritance_Without_Persisting_A_Link()
    {
        var id = Guid.NewGuid();
        await WithUnitOfWorkAsync(async () =>
            await GetRequiredService<ICalendarRepository>().InsertAsync(
                new Calendars.Calendar(id, null, "Public", CalendarKind.Public, "UTC"), autoSave: true));

        var error = await Should.ThrowAsync<BusinessException>(() => WithUnitOfWorkAsync(async () =>
        {
            var calendar = await GetRequiredService<ICalendarRepository>().GetAsync(id);
            await GetRequiredService<CalendarManager>().AddInheritanceAsync(calendar, calendar);
        }));
        error.Code.ShouldBe(CalendarErrorCodes.CalendarCannotInheritItself);

        await WithUnitOfWorkAsync(async () =>
            (await GetRequiredService<ICalendarRepository>().GetInheritedCalendarIdsAsync(id)).ShouldBeEmpty());
    }

    private Task CreateDefaultAsync(Guid tenantId) => WithUnitOfWorkAsync(async () =>
    {
        var calendar = await GetRequiredService<CalendarManager>().CreateAsync(Guid.NewGuid(), tenantId,
            "Default", CalendarKind.Default, "UTC", isDefault: true);
        await GetRequiredService<ICalendarRepository>().InsertAsync(calendar, autoSave: true);
    });
}
