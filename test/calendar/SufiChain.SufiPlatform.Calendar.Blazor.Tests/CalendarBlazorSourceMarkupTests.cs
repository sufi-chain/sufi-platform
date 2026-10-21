using System.Text.RegularExpressions;
using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.Calendar.Blazor.Tests;

/// <summary>
/// Source-markup proof for sufi-chain#394: the create-event page paints its form (SCR-17), the
/// week view is an hour x day time grid (SCR-16), and the calendar surface resolves a time zone
/// without throwing.
///
/// The markup and code-behind are asserted directly rather than through bUnit. The components
/// derive from SufiComponentBase (OwningComponentBase + the whole platform service surface), and
/// a bUnit render host for them crashes the test process with a Blazor render-recursion stack
/// overflow. This is the same source-markup pattern the AI and CMS suites already use.
/// </summary>
public class CalendarBlazorSourceMarkupTests
{
    private const string BlazorPublic = "modules/calendar/src/SufiChain.SufiPlatform.Calendar.Blazor.Public";

    private const string EventEditor = BlazorPublic + "/Components/SufiEventEditor.razor";
    private const string EventEditorCode = BlazorPublic + "/Components/SufiEventEditor.razor.cs";
    private const string CalendarView = BlazorPublic + "/Components/SufiCalendarView.razor";
    private const string CalendarViewCode = BlazorPublic + "/Components/SufiCalendarView.razor.cs";
    private const string EventPage = BlazorPublic + "/Pages/CalendarEventPage.razor";

    [Fact]
    public void Create_Event_Page_Composes_The_Event_Editor_On_Every_Route()
    {
        var markup = Normalized(Read(EventPage));

        // The three create/edit entry points all mount the editor (SCR-17).
        foreach (var route in new[]
                 {
                     "/panel/admin/calendar/events/new",
                     "/panel/portal/calendar/events/new",
                     "/panel/portal/calendar-pro/events/new"
                 })
        {
            markup.ShouldContain($"@page \"{route}\"");
        }

        markup.ShouldContain("<SufiEventEditor");
        markup.ShouldContain("TimeZoneId=\"TimeZoneId\"");
    }

    [Fact]
    public void Event_Editor_Renders_The_Form_Body_Not_Only_The_Footer()
    {
        var markup = Normalized(Read(EventEditor));

        // The form body is rendered only when the editor is ready; the footer renders either way.
        // That guard is what turned a load failure into a body-less page (SCR-17).
        markup.ShouldContain("@if (_ready)");
        markup.ShouldContain("<EditForm Model=\"_model\" OnValidSubmit=\"SaveAsync\">");

        // The business fields the form must paint.
        markup.ShouldContain("Label=\"@L[\"Title\"]\"");
        markup.ShouldContain("<SbDateRangePicker");
        markup.ShouldContain("<SbTimePicker");
        markup.ShouldContain("Label=\"@L[\"TimeZone\"]\"");
        markup.ShouldContain("Label=\"@L[\"Status\"]\"");
        markup.ShouldContain("Label=\"@L[\"Description\"]\"");

        // The load-failed branch offers a retry instead of leaving the body blank.
        markup.ShouldContain("@if (_loadFailed)");
        markup.ShouldContain("RetryLoadAsync");
        markup.ShouldContain("L[\"EventEditorLoadFailed\"]");
        markup.ShouldContain("L[\"EventEditorRetry\"]");
    }

    [Fact]
    public void Event_Editor_Resolves_The_Time_Zone_Without_Throwing()
    {
        var code = Read(EventEditorCode);

        code.ShouldContain("CalendarTimeZoneResolver.Resolve(_model.TimeZoneId, TimeZoneId)");
        code.ShouldNotContain("TimeZoneInfo.FindSystemTimeZoneById");
    }

    [Fact]
    public void Week_View_Renders_A_24_Hour_Grid_Across_The_Visible_Days()
    {
        var markup = Normalized(Read(CalendarView));

        // The week branch is the only one that paints the hour x day grid (SCR-16).
        markup.ShouldContain("View == SufiCalendarViewMode.Week");
        markup.ShouldContain("sufi-calendar-view__week\" role=\"grid\"");
        markup.ShouldContain("sufi-calendar-view__week-row--head");
        markup.ShouldContain("sufi-calendar-view__week-row--all-day");
        markup.ShouldContain("sufi-calendar-view__week-body");

        // Every hour of the day gets a row, and every day gets a clickable slot in that row.
        markup.ShouldContain("@foreach (var hour in DayTimelineHours)");
        markup.ShouldContain("sufi-calendar-view__week-hour");
        markup.ShouldContain("sufi-calendar-view__week-slot");
        markup.ShouldContain("SelectDayHourSlotAsync(slotDay, rowHour)");

        // Column headers per visible day.
        markup.ShouldContain("GetWeekHeadClass(headDay)");
        markup.ShouldContain("role=\"columnheader\"");
    }

    [Fact]
    public void Week_View_Hour_Grid_Spans_A_Full_Day()
    {
        var code = Read(CalendarViewCode);

        // 24 rows: 00:00 .. 23:00.
        code.ShouldContain("DayTimelineHours => Enumerable.Range(0, 24)");
        code.ShouldContain("SelectDayHourSlotAsync(DateTime day, int hour)");
        code.ShouldContain("day.Date.AddHours(hour)");
        code.ShouldContain("sufi-calendar-view__week-head-cell");
    }

    [Fact]
    public void Calendar_Surface_Resolves_Its_Time_Zone_Without_Throwing()
    {
        // Regression guard for the remaining gap in #394: the week/day grid used the throwing
        // FindSystemTimeZoneById, so a browser-supplied IANA id took the whole grid down.
        Read(CalendarViewCode).ShouldContain("CalendarTimeZoneResolver.Resolve(TimeZoneId)");

        foreach (var relative in new[]
                 {
                     BlazorPublic + "/Components/SufiFreeBusyPicker.razor.cs",
                     BlazorPublic + "/Components/CalendarToolbarWidget.razor.cs"
                 })
        {
            var code = Read(relative);
            code.ShouldContain("CalendarTimeZoneResolver.Resolve(");
            code.ShouldNotContain("TimeZoneInfo.FindSystemTimeZoneById");
        }
    }

    [Fact]
    public void Week_Grid_Has_Responsive_Styles()
    {
        // The grid is laid out with CSS; without these rules the markup is unstyled and invisible.
        var css = Read(BlazorPublic + "/wwwroot/calendar-public.css");

        css.ShouldContain(".sufi-calendar-view__week-row {");
        css.ShouldContain(".sufi-calendar-view__week-slot");
        css.ShouldContain(".sufi-calendar-view__week-hour");
        css.ShouldContain("grid-template-columns: 3.5rem repeat(7, minmax(5.5rem, 1fr))");
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

    // Collapse runs of whitespace so an indentation change does not break a structural assertion.
    private static string Normalized(string source) => Regex.Replace(source, "\\s+", " ");
}
