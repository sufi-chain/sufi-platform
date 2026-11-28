using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.Calendar.Blazor.Tests;

/// <summary>
/// A calendar load failure stays on the localized alert. The assistant permission check does not open a chat session.
/// </summary>
public class CalendarCopilotSourceMarkupTests
{
    [Fact]
    public void Calendar_Surfaces_Show_A_Localized_Load_Failure()
    {
        var view = Read("modules/calendar/src/SufiChain.SufiPlatform.Calendar.Blazor.Public/Components/SufiCalendarView.razor");
        var scheduler = Read("modules/calendar/src/SufiChain.SufiPlatform.Calendar.Blazor.Public/Components/SufiCalendarScheduler.razor");
        var viewCode = Read("modules/calendar/src/SufiChain.SufiPlatform.Calendar.Blazor.Public/Components/SufiCalendarView.razor.cs");
        var schedulerCode = Read("modules/calendar/src/SufiChain.SufiPlatform.Calendar.Blazor.Public/Components/SufiCalendarScheduler.razor.cs");

        view.ShouldContain("L[\"CalendarLoadFailed\"]");
        scheduler.ShouldContain("L[\"CalendarLoadFailed\"]");
        viewCode.ShouldContain("FailOccurrenceLoad(exception)");
        viewCode.ShouldNotContain("throw exception");
        schedulerCode.ShouldContain("LoadFailed = true");
        schedulerCode.ShouldNotContain("throw exception");
    }

    [Fact]
    public void Calendar_Page_Hides_A_Load_Failure_And_Does_Not_Open_A_Session_While_Checking_Access()
    {
        var code = Read("modules/calendar/src/SufiChain.SufiPlatform.Calendar.Assistant.Blazor/Pages/CalendarHooshvarePage.razor.cs");

        code.ShouldContain("DisableHooshvare(\"CalendarHooshvare:LoadFailed\")");
        code.ShouldContain("DisableHooshvare(\"CalendarHooshvare:Unauthorized\")");
        code.ShouldContain("Calendar page could not be initialized.");

        var start = code.IndexOf("CanUseHooshvareAsync()", StringComparison.Ordinal);
        var end = code.IndexOf("IsGrantedAnyAsync", start, StringComparison.Ordinal);
        start.ShouldBeGreaterThanOrEqualTo(0);
        end.ShouldBeGreaterThan(start);
        var permissionCheck = code[start..end];
        permissionCheck.ShouldContain("CalendarFeatures.Assistant");
        permissionCheck.ShouldContain("LicenseProducts.Calendar");
        permissionCheck.ShouldNotContain("CreateAsync");
        permissionCheck.ShouldNotContain("ChatSessionAppService");
    }

    [Fact]
    public void Shim_Still_Registers_The_Assistant_Router_And_Menu()
    {
        var shim = Read("pro-modules/calendar-hooshvare/src/SufiChain.SufiPlatform.Calendar.Hooshvare.Blazor/CalendarHooshvareBlazorModule.cs");
        var assistant = Read("modules/calendar/src/SufiChain.SufiPlatform.Calendar.Assistant.Blazor/SufiCalendarAssistantBlazorModule.cs");
        var page = Read("modules/calendar/src/SufiChain.SufiPlatform.Calendar.Assistant.Blazor/Pages/CalendarHooshvarePage.razor");

        shim.ShouldContain("typeof(SufiCalendarAssistantBlazorModule)");
        assistant.ShouldContain("AdditionalAssemblies.Add(typeof(SufiCalendarAssistantBlazorModule).Assembly)");
        assistant.ShouldContain("new CalendarHooshvareMenuContributor()");
        page.ShouldContain("@page \"/panel/portal/calendar\"");
        page.ShouldContain("@page \"/panel/portal/calendar/mobile\"");
        page.ShouldContain("@page \"/panel/portal/calendar-pro\"");
        page.ShouldContain("@page \"/panel/portal/calendar-pro/mobile\"");

        var portal = Read("modules/calendar/src/SufiChain.SufiPlatform.Calendar.Blazor.Public/Pages/CalendarPage.razor");
        portal.ShouldNotContain("@page \"/panel/portal/calendar\"");

        var host = Read("hosts/SufiChane.SufiPlatform/src/SufiChane.SufiPlatform.Blazor.WebApp/SufiPlatformPanelCompositionModule.cs");
        host.ShouldContain("From = \"/panel/portal/calendar-pro\"");
        host.ShouldContain("To = \"/panel/portal/calendar\"");
        host.ShouldNotContain("To = \"/panel/portal/calendar-pro\"");
        host.ShouldContain("\"SufiPlatform.Calendar\"");
        host.ShouldContain("\"/panel/portal/calendar/mobile\"");
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

            candidate = Path.Combine(directory.FullName, "sufi-platform", relative);
            if (File.Exists(candidate))
            {
                return File.ReadAllText(candidate);
            }
        }

        throw new FileNotFoundException(relative);
    }
}
