using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.Application.Dtos;
using SufiChain.SufiPlatform.Calendar.AI.Tools;
using SufiChain.SufiPlatform.Calendar.Calendars;
using SufiChain.SufiPlatform.Calendar.Events;
using Xunit;

namespace SufiChain.SufiPlatform.Calendar;

public class CalendarSearchEventsLogTests
{
    [Fact]
    public async Task CalendarToolResults_AreLogged()
    {
        var eventId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
        var calendarId = Guid.Parse("11111111-2222-3333-4444-555555555555");
        var start = new DateTime(2026, 11, 28, 9, 0, 0, DateTimeKind.Utc);
        var end = start.AddHours(1);
        var events = Substitute.For<ICalendarEventAppService>();
        events.GetListAsync(Arg.Any<GetEventListInput>()).Returns(_ => Task.FromResult(new PagedResultDto<CalendarEventDto>(1,
        [
            new CalendarEventDto
            {
                Id = eventId,
                CalendarId = calendarId,
                Title = "Standup",
                StartUtc = start,
                EndUtc = end,
                TimeZoneId = "UTC"
            }
        ])));
        var catalog = Substitute.For<ICalendarCatalogIntegrationService>();
        catalog.GetVisibleCalendarsAsync(Arg.Any<string?>()).Returns(
        [
            new CalendarLookupDto { Id = calendarId, Name = "Personal", Kind = CalendarKind.Personal }
        ]);
        var logger = new CaptureLogger();
        var tool = new CalendarSearchEventsTool(events, catalog, logger);

        await tool.SearchEventsAsync();

        var message = logger.Messages.ShouldHaveSingleItem();
        message.ShouldContain("post-filter live set");
        message.ShouldContain(eventId.ToString());
        message.ShouldContain("Standup");
        message.ShouldContain(start.ToString("o"));
        message.ShouldContain(end.ToString("o"));
        message.ShouldContain(calendarId.ToString());
        message.ShouldContain("Personal");
    }

    private sealed class CaptureLogger : ILogger<CalendarSearchEventsTool>
    {
        public List<string> Messages { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (logLevel == LogLevel.Information)
            {
                Messages.Add(formatter(state, exception));
            }
        }
    }
}
