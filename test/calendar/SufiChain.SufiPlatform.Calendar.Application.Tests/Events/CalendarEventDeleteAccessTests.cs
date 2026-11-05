using Shouldly;
using SufiChain.SufiPlatform.Calendar.Events;
using Volo.Abp;
using Volo.Abp.Authorization;
using Xunit;

namespace SufiChain.SufiPlatform.Calendar.Events;

public class CalendarEventDeleteAccessTests
{
    [Fact]
    public void Owner_Is_The_Creator_Or_The_Calendar_Owner()
    {
        var creator = Guid.NewGuid();
        var calendarOwner = Guid.NewGuid();

        CalendarEventDeleteAccess.IsOwner(creator, creator, calendarOwner).ShouldBeTrue();
        CalendarEventDeleteAccess.IsOwner(calendarOwner, creator, calendarOwner).ShouldBeTrue();
        CalendarEventDeleteAccess.IsOwner(Guid.NewGuid(), creator, calendarOwner).ShouldBeFalse();
        CalendarEventDeleteAccess.IsOwner(null, creator, calendarOwner).ShouldBeFalse();
    }

    [Fact]
    public void Stranger_Without_Delete_Permission_Gets_A_Business_Error()
    {
        var exception = Should.Throw<BusinessException>(() => CalendarEventDeleteAccess.Ensure(isOwner: false, hasDeletePermission: false));

        exception.Code.ShouldBe(CalendarErrorCodes.DeleteNotAllowed);
        exception.ShouldNotBeOfType<AbpAuthorizationException>();
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void Owner_Or_Delete_Permission_Is_Allowed(bool isOwner, bool hasDeletePermission)
    {
        CalendarEventDeleteAccess.Ensure(isOwner, hasDeletePermission);
    }
}
