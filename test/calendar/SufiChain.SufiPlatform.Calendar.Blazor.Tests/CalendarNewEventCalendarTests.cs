using Shouldly;
using SufiChain.SufiPlatform.Calendar.Blazor.Public.Components;
using Xunit;

namespace SufiChain.SufiPlatform.Calendar.Blazor.Tests;

public class CalendarNewEventCalendarTests
{
    private static readonly Guid Personal = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid Requested = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    [Fact]
    public void EventsNew_WithoutCalendarId_UsesDefaultCalendarOrRequiresSelection()
    {
        CalendarNewEventCalendar.Resolve(Guid.Empty, Personal).ShouldBe(Personal);
        CalendarNewEventCalendar.RequiresSelection(Guid.Empty).ShouldBeTrue();
    }

    [Fact]
    public void CreateEvent_WithDefaultCalendar_Succeeds()
    {
        var resolved = CalendarNewEventCalendar.Resolve(Guid.Empty, Personal);
        CalendarNewEventCalendar.RequiresSelection(resolved).ShouldBeFalse();
        resolved.ShouldBe(Personal);
        CalendarNewEventCalendar.Resolve(Requested, Personal).ShouldBe(Requested);
    }

    [Fact]
    public void EventsNew_WithoutCalendarId_DoesNotShowGenericErrorModal()
    {
        var save = SaveSlice();
        save.ShouldContain("CalendarNewEventCalendar.Resolve");
        save.ShouldContain("CalendarNewEventCalendar.RequiresSelection");
        save.ShouldNotContain("An error occurred");
    }

    [Fact]
    public void CalendarNotAccessible_IsShownLocalized()
    {
        var save = SaveSlice();
        save.ShouldContain("CalendarErrorCodes.CalendarNotAccessible");
        save.ShouldContain("L[\"CalendarNotAccessible\"]");

        foreach (var culture in new[] { "en", "fa", "ar", "es" })
        {
            var json = Read($"modules/calendar/src/SufiChain.SufiPlatform.Calendar.Domain.Shared/Localization/Calendar/{culture}.json");
            json.ShouldContain("\"CalendarNotAccessible\"");
        }

        Read("modules/calendar/src/SufiChain.SufiPlatform.Calendar.Domain.Shared/Localization/Calendar/ar.json")
            .ShouldContain("ليست لديك صلاحية الوصول إلى هذا التقويم.");
    }

    private static string SaveSlice()
    {
        var code = Read("modules/calendar/src/SufiChain.SufiPlatform.Calendar.Blazor.Public/Components/SufiEventEditor.razor.cs");
        var start = code.IndexOf("Task SaveAsync()", StringComparison.Ordinal);
        var end = code.IndexOf("Task DeleteAsync()", start, StringComparison.Ordinal);
        start.ShouldBeGreaterThanOrEqualTo(0);
        end.ShouldBeGreaterThan(start);
        return code[start..end];
    }

    private static string Read(string relative)
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory != null; directory = directory.Parent)
        {
            foreach (var root in new[] { directory.FullName, Path.Combine(directory.FullName, "sufi-platform") })
            {
                var candidate = Path.Combine(root, relative);
                if (File.Exists(candidate))
                {
                    return File.ReadAllText(candidate);
                }
            }
        }

        throw new FileNotFoundException(relative);
    }
}
