using System.Globalization;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.Calendar.Availability;
using SufiChain.SufiPlatform.Calendar.Blazor.Public.Components;
using SufiChain.SufiPlatform.Calendar.Events;
using Xunit;

namespace SufiChain.SufiPlatform.Calendar.Blazor.Tests;

/// <summary>
/// Render proof for sufi-chain#394: the create-event page must paint its form (SCR-17) and the
/// week view must paint an hour x day time grid (SCR-16).
/// </summary>
public class CalendarBlazorRenderTests : CalendarBlazorTestContext
{
    [Fact]
    public void Create_Event_Page_Renders_The_Form_Body()
    {
        var cut = Render<SufiEventEditor>(parameters => parameters
            .Add(component => component.CalendarId, Guid.NewGuid())
            .Add(component => component.TimeZoneId, "UTC"));

        cut.WaitForAssertion(() =>
        {
            // The body is the form, not just the Save/Cancel footer (the SCR-17 symptom).
            cut.FindAll(".sufi-event-editor").Count.ShouldBe(1);
            cut.Find("form").ShouldNotBeNull();

            // The title field paints as a real input, so the body is not just the footer chrome.
            cut.FindAll("input").Count.ShouldBeGreaterThanOrEqualTo(1);

            // A blank body shows the load-failed alert; a healthy new-event form shows none.
            cut.FindAll("[role='alert']").Count.ShouldBe(0);
        });

        cut.Find(".sufi-event-editor-page__footer").ShouldNotBeNull();
    }

    [Fact]
    public void Week_View_Renders_A_24_Hour_Grid_Across_Seven_Days()
    {
        var cut = Render<SufiCalendarView>(parameters => parameters
            .Add(component => component.View, SufiCalendarViewMode.Week)
            .Add(component => component.Date, new DateTime(2026, 10, 21))
            .Add(component => component.TimeZoneId, "UTC"));

        cut.WaitForAssertion(() =>
        {
            cut.FindAll(".sufi-calendar-view__week").Count.ShouldBe(1);

            // Head row + all-day row + 24 hour rows.
            cut.FindAll(".sufi-calendar-view__week-row").Count.ShouldBe(26);

            // 24 hours x 7 days of clickable slots.
            cut.FindAll(".sufi-calendar-view__week-slot").Count.ShouldBe(24 * 7);

            // One column header per day.
            cut.FindAll(".sufi-calendar-view__week-head-cell").Count.ShouldBe(7);
        });

        cut.FindAll("[role='grid']").Count.ShouldBe(1);
        cut.FindAll("[role='columnheader']").Count.ShouldBe(7);
    }

    [Fact]
    public void Week_View_Survives_A_Time_Zone_The_Host_Cannot_Resolve()
    {
        // A browser-supplied IANA id (or a row written on another host) must not take the grid down.
        var cut = Render<SufiCalendarView>(parameters => parameters
            .Add(component => component.View, SufiCalendarViewMode.Week)
            .Add(component => component.Date, new DateTime(2026, 10, 21))
            .Add(component => component.TimeZoneId, "Not/AZone"));

        cut.WaitForAssertion(() => cut.FindAll(".sufi-calendar-view__week-slot").Count.ShouldBe(24 * 7));
    }
}

/// <summary>
/// Shared bUnit host: a key-based localizer so assertions stay culture independent, plus the
/// calendar application services the components inject.
/// </summary>
public abstract class CalendarBlazorTestContext : BunitContext
{
    protected CalendarBlazorTestContext()
    {
        Services.AddSingleton<IStringLocalizerFactory>(new KeyLocalizerFactory());
        Services.AddSingleton(typeof(IStringLocalizer<>), typeof(KeyLocalizer<>));
        Services.AddSingleton(typeof(ILoggerFactory), NullLoggerFactory.Instance);
        Services.AddSingleton(Substitute.For<ICalendarEventAppService>());
        Services.AddSingleton(Substitute.For<IAvailabilityCalendarAppService>());
    }

    private sealed class KeyLocalizerFactory : IStringLocalizerFactory
    {
        public IStringLocalizer Create(Type resourceSource) => new KeyLocalizer();

        public IStringLocalizer Create(string baseName, string location) => new KeyLocalizer();
    }
}

internal class KeyLocalizer : IStringLocalizer
{
    public LocalizedString this[string name] => new(name, name, resourceNotFound: false);

    public LocalizedString this[string name, params object[] arguments] =>
        new(name, string.Format(CultureInfo.InvariantCulture, name, arguments), resourceNotFound: false);

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) =>
        Array.Empty<LocalizedString>();
}

internal sealed class KeyLocalizer<T> : KeyLocalizer, IStringLocalizer<T>
{
}
