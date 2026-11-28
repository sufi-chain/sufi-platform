using Shouldly;
using SufiChain.SufiPlatform.Calendar.Blazor.Public;
using Xunit;

namespace SufiChain.SufiPlatform.Calendar.Blazor.Tests;

public class CalendarCreateNavigationTests
{
    [Fact]
    public void CreateEvent_NavigatesToEditOrCalendar_DoesNotResetForm()
    {
        var id = Guid.Parse("11111111-1111-1111-1111-111111111111");

        CalendarPageRoutes.CreatedEventUrl("/panel/portal/calendar/events/new?calendarId=abc", id)
            .ShouldBe("/panel/portal/calendar/events/11111111-1111-1111-1111-111111111111");
        CalendarPageRoutes.CreatedEventUrl("/panel/admin/calendar/events/new", id)
            .ShouldBe("/panel/admin/calendar/events/11111111-1111-1111-1111-111111111111");
        CalendarPageRoutes.CreatedEventUrl("/panel/portal/calendar-pro/events/new", id)
            .ShouldBe("/panel/portal/calendar-pro/events/11111111-1111-1111-1111-111111111111");
    }

    [Fact]
    public void CreateEvent_DoesNotUseNewEventUrlAsReturn()
    {
        CalendarPageRoutes.IsSafeReturnUrl("/panel/portal/calendar/events/new").ShouldBeFalse();
        CalendarPageRoutes.IsSafeReturnUrl("/panel/portal/calendar/events/new?calendarId=abc").ShouldBeFalse();
        CalendarPageRoutes.IsSafeReturnUrl("/panel/admin/calendar/events/new").ShouldBeFalse();
        CalendarPageRoutes.IsSafeReturnUrl("/panel/portal/calendar").ShouldBeTrue();

        CalendarPageRoutes.ResolveReturnUrl(
                "/panel/portal/calendar/events/new",
                "/panel/portal/calendar/events/new")
            .ShouldBe("/panel/portal/calendar");
    }
}
