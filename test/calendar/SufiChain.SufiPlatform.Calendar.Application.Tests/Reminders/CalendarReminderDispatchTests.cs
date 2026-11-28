using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.Calendar.Events;
using SufiChain.SufiPlatform.Calendar.Localization;
using SufiChain.SufiPlatform.Calendar.Reminders;
using SufiChain.SufiPlatform.SufiCom;
using SufiChain.SufiPlatform.SufiCom.Email;
using SufiChain.SufiPlatform.SufiCom.Notifications;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Settings;
using Volo.Abp.Threading;
using Volo.Abp.Timing;
using Volo.Abp.Uow;
using Xunit;

namespace SufiChain.SufiPlatform.Calendar.Reminders;

public class CalendarReminderDispatchTests
{
    private static readonly DateTime Now = new(2026, 11, 28, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Worker_IsRegistered_AndCallsDispatcher()
    {
        var module = Read("modules/calendar/src/SufiChain.SufiPlatform.Calendar.Application/SufiCalendarApplicationModule.cs");
        module.ShouldContain("AddBackgroundWorkerAsync<CalendarReminderDispatchWorker>()");

        var dispatcher = Substitute.For<IEventReminderDispatcher>();
        dispatcher.DispatchDueAsync(Arg.Any<DateTime>(), Arg.Any<CancellationToken>()).Returns(2);
        var worker = new ProbeWorker(ScopeFactory(Services(dispatcher, Now)));

        await worker.RunAsync();

        await dispatcher.Received(1).DispatchDueAsync(Now, Arg.Any<CancellationToken>());
        worker.PeriodMilliseconds.ShouldBe(CalendarReminderSettings.DefaultDispatchIntervalSeconds * 1000);
    }

    [Fact]
    public async Task DispatchDue_InApp_PublishesInboxNotification()
    {
        var userId = Guid.NewGuid();
        var item = Item(ReminderChannel.InApp, userId, "ada@example.com", TimeSpan.FromMinutes(-2), Now.AddMinutes(-1), Now.AddMinutes(1));
        var publisher = new RecordingPublisher();
        var repository = Repository(item);
        var dispatcher = Dispatcher(repository, publisher, new RecordingEmailSender(), FaLocalizer());

        var count = await dispatcher.DispatchDueAsync(Now);

        count.ShouldBe(1);
        publisher.Titles.Single().ShouldBe("یادآوری: Planning");
        publisher.UserIds.Single().ShouldBe(userId);
        publisher.Categories.Single().ShouldBe(EventReminderDispatcher.InAppCategory);
        publisher.Sources.Single().ShouldBe(EventReminderDispatcher.InAppSource);
        publisher.Urls.Single().ShouldBe(EventReminderDispatcher.EventUrl(item.Event.Id));
        item.Reminder.SentAtUtc.ShouldBe(Now);
    }

    [Fact]
    public async Task DispatchDue_InApp_NoUser_NotMarkedSent()
    {
        var item = Item(ReminderChannel.InApp, null, "ada@example.com", TimeSpan.FromMinutes(-2), Now.AddMinutes(-1), Now.AddMinutes(1));
        var publisher = new RecordingPublisher();
        var dispatcher = Dispatcher(Repository(item), publisher, new RecordingEmailSender(), FaLocalizer());

        var count = await dispatcher.DispatchDueAsync(Now);

        count.ShouldBe(0);
        publisher.Titles.ShouldBeEmpty();
        item.Reminder.SentAtUtc.ShouldBeNull();
    }

    [Fact]
    public async Task DispatchDue_DueThreeMinutesAgo_WithinCatchUp_Fires()
    {
        var start = Now.AddMinutes(-1);
        var item = Item(ReminderChannel.Email, Guid.NewGuid(), "ada@example.com", TimeSpan.FromMinutes(-2), start, start.AddSeconds(90));
        var email = new RecordingEmailSender();
        var dispatcher = Dispatcher(Repository(item), new RecordingPublisher(), email, FaLocalizer());

        var count = await dispatcher.DispatchDueAsync(Now);

        count.ShouldBe(1);
        email.Queued.ShouldBe(1);
        item.Reminder.SentAtUtc.ShouldBe(Now);
    }

    [Fact]
    public async Task DispatchDue_DuePastCatchUp_MarkedSkipped_NotReloaded()
    {
        var start = Now.AddMinutes(-30);
        var item = Item(ReminderChannel.Email, Guid.NewGuid(), "ada@example.com", TimeSpan.FromMinutes(-10), start, start.AddMinutes(10), skip: true);
        var email = new RecordingEmailSender();
        var repository = Repository(item);
        var dispatcher = Dispatcher(repository, new RecordingPublisher(), email, FaLocalizer());

        var first = await dispatcher.DispatchDueAsync(Now);
        var second = await dispatcher.DispatchDueAsync(Now);

        first.ShouldBe(0);
        second.ShouldBe(0);
        email.Queued.ShouldBe(0);
        item.Reminder.SentAtUtc.ShouldBe(Now);
        CalendarReminderSkip.IsRecorded(item.Event, item.Reminder.Id).ShouldBeTrue();
        await repository.Received(2).GetDueRemindersAsync(Now, Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DispatchDue_EmailSubject_Localized()
    {
        var item = Item(ReminderChannel.Email, Guid.NewGuid(), "ada@example.com", TimeSpan.FromMinutes(-2), Now.AddMinutes(-1), Now.AddMinutes(30));
        var email = new RecordingEmailSender();
        var dispatcher = Dispatcher(Repository(item), new RecordingPublisher(), email, FaLocalizer());

        await dispatcher.DispatchDueAsync(Now);

        email.Subject.ShouldBe("یادآوری: Planning");
        email.Subject.ShouldNotStartWith("Reminder:");
        email.Body.ShouldContain(CalendarTimeZoneResolver.Resolve(item.Event.TimeZoneId, "Asia/Tehran").Id);
        foreach (var culture in new[] { "en", "fa", "ar", "es" })
        {
            var json = Read($"modules/calendar/src/SufiChain.SufiPlatform.Calendar.Domain.Shared/Localization/Calendar/{culture}.json");
            json.ShouldContain("\"Reminder:EmailSubject\"");
            json.ShouldContain("\"Reminder:EmailBody\"");
            json.ShouldContain("\"Reminder:InAppTitle\"");
            json.ShouldContain("\"Reminder:InAppBody\"");
        }
    }

    private static EventReminderDispatcher Dispatcher(
        ICalendarEventRepository repository,
        INotificationPublisher publisher,
        IEmailSender email,
        IStringLocalizer<CalendarResource> localizer)
    {
        var settings = Substitute.For<ISettingProvider>();
        settings.GetOrNullAsync(CalendarReminderSettings.CatchUpMinutes).Returns("15");
        return new EventReminderDispatcher(repository, email, publisher, settings, localizer, NullLogger<EventReminderDispatcher>.Instance);
    }

    private static ICalendarEventRepository Repository(EventReminderDispatchItem item)
    {
        var pending = new List<EventReminderDispatchItem> { item };
        var repository = Substitute.For<ICalendarEventRepository>();
        repository.GetDueRemindersAsync(Arg.Any<DateTime>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
            .Returns(_ => pending.Where(x => x.Reminder.SentAtUtc == null).ToList());
        repository.UpdateAsync(Arg.Any<CalendarEvent>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<CalendarEvent>());
        return repository;
    }

    private static EventReminderDispatchItem Item(
        ReminderChannel channel,
        Guid? userId,
        string? email,
        TimeSpan offset,
        DateTime start,
        DateTime end,
        bool skip = false)
    {
        var calendarEvent = new CalendarEvent(Guid.NewGuid(), null, Guid.NewGuid(), "Planning", start, end, false, "Asia/Tehran");
        var attendee = new EventAttendee(Guid.NewGuid(), calendarEvent.Id, userId, email, "Ada", AttendeeRole.Organizer);
        calendarEvent.AddAttendee(attendee);
        var reminder = new EventReminder(Guid.NewGuid(), calendarEvent.Id, offset, channel, attendee.Id);
        calendarEvent.AddReminder(reminder);
        var occurrence = new EventOccurrence(
            calendarEvent.Id,
            calendarEvent.CalendarId,
            calendarEvent.Title,
            start,
            start,
            end,
            false,
            calendarEvent.TimeZoneId,
            EventStatus.Confirmed,
            null,
            null,
            null,
            null,
            null);
        return new EventReminderDispatchItem(calendarEvent, reminder, occurrence, attendee, start.Add(offset), skip);
    }

    private static DictionaryLocalizer FaLocalizer() => new(new Dictionary<string, string>
    {
        ["Reminder:EmailSubject"] = "یادآوری: {0}",
        ["Reminder:EmailBody"] = "{0}\nشروع: {1}\nمکان: {2}",
        ["Reminder:InAppTitle"] = "یادآوری: {0}",
        ["Reminder:InAppBody"] = "{0} در {1} شروع می‌شود.",
        ["EmailSender:DisplayName"] = "تقویم"
    });

    private static IServiceProvider Services(IEventReminderDispatcher dispatcher, DateTime now)
    {
        var settings = Substitute.For<ISettingProvider>();
        settings.GetOrNullAsync(Arg.Any<string>()).Returns((string?)null);
        var current = Substitute.For<ICurrentTenant>();
        current.Change(Arg.Any<Guid?>(), Arg.Any<string?>()).Returns(Substitute.For<IDisposable>());
        var unitOfWork = Substitute.For<IUnitOfWork>();
        unitOfWork.CompleteAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        var unitOfWorkManager = Substitute.For<IUnitOfWorkManager>();
        unitOfWorkManager.Begin(Arg.Any<AbpUnitOfWorkOptions>(), Arg.Any<bool>()).Returns(unitOfWork);
        var clock = Substitute.For<IClock>();
        clock.Now.Returns(now);

        var provider = Substitute.For<IServiceProvider>();
        provider.GetService(typeof(ISettingProvider)).Returns(settings);
        provider.GetService(typeof(ICurrentTenant)).Returns(current);
        provider.GetService(typeof(IUnitOfWorkManager)).Returns(unitOfWorkManager);
        provider.GetService(typeof(IEventReminderDispatcher)).Returns(dispatcher);
        provider.GetService(typeof(IClock)).Returns(clock);
        return provider;
    }

    private static IServiceScopeFactory ScopeFactory(IServiceProvider provider)
    {
        var factory = Substitute.For<IServiceScopeFactory>();
        var scope = Substitute.For<IServiceScope>();
        scope.ServiceProvider.Returns(provider);
        factory.CreateScope().Returns(scope);
        return factory;
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

    private sealed class ProbeWorker : CalendarReminderDispatchWorker
    {
        public ProbeWorker(IServiceScopeFactory scopeFactory)
            : base(new AbpAsyncTimer(), scopeFactory)
        {
            var lazy = Substitute.For<IAbpLazyServiceProvider>();
            lazy.LazyGetService<ILogger>(Arg.Any<Func<IServiceProvider, object>>()).Returns(NullLogger.Instance);
            LazyServiceProvider = lazy;
        }

        public int PeriodMilliseconds => Timer.Period;

        public Task RunAsync() => DoWorkAsync(new PeriodicBackgroundWorkerContext(ServiceScopeFactory.CreateScope().ServiceProvider));
    }

    private sealed class RecordingPublisher : INotificationPublisher
    {
        public List<string> Titles { get; } = new();

        public List<Guid> UserIds { get; } = new();

        public List<string?> Categories { get; } = new();

        public List<string?> Sources { get; } = new();

        public List<string?> Urls { get; } = new();

        public Task PublishAsync(InboxNotificationEto notification) => Task.CompletedTask;

        public Task PublishAsync(
            string title,
            string? body,
            IEnumerable<Guid> userIds,
            InboxNotificationSeverity severity = InboxNotificationSeverity.Info,
            string? category = null,
            string? source = null,
            string? url = null,
            Dictionary<string, string>? data = null)
        {
            Titles.Add(title);
            UserIds.AddRange(userIds);
            Categories.Add(category);
            Sources.Add(source);
            Urls.Add(url);
            return Task.CompletedTask;
        }

        public Task PublishToAllAsync(
            string title,
            string? body,
            InboxNotificationSeverity severity = InboxNotificationSeverity.Info,
            string? category = null,
            string? source = null,
            string? url = null,
            Dictionary<string, string>? data = null) => Task.CompletedTask;

        public Task PublishAsync(NotificationEnvelope envelope, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class RecordingEmailSender : IEmailSender
    {
        public MessageType MessageType => MessageType.Email;

        public string? Subject { get; private set; }

        public string? Body { get; private set; }

        public int Queued { get; private set; }

        public Task QueueAsync(
            string to,
            string subject,
            string body,
            bool isBodyHtml = true,
            string? from = null,
            string? replyTo = null,
            IEnumerable<string>? cc = null,
            IEnumerable<string>? bcc = null,
            IEnumerable<MessageAttachment>? attachments = null,
            AdditionalMessageSendingArgs? additionalArgs = null)
        {
            Queued++;
            Subject = subject;
            Body = body;
            return Task.CompletedTask;
        }

        public Task SendAsync(
            string to,
            string subject,
            string body,
            bool isBodyHtml = true,
            string? from = null,
            string? replyTo = null,
            IEnumerable<string>? cc = null,
            IEnumerable<string>? bcc = null,
            IEnumerable<MessageAttachment>? attachments = null,
            AdditionalMessageSendingArgs? additionalArgs = null) =>
            QueueAsync(to, subject, body, isBodyHtml, from, replyTo, cc, bcc, attachments, additionalArgs);
    }

    private sealed class DictionaryLocalizer : IStringLocalizer<CalendarResource>
    {
        private readonly Dictionary<string, string> _values;

        public DictionaryLocalizer(Dictionary<string, string> values) => _values = values;

        public LocalizedString this[string name] => new(name, _values.TryGetValue(name, out var value) ? value : name);

        public LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                var template = _values.TryGetValue(name, out var value) ? value : name;
                return new LocalizedString(name, string.Format(CultureInfo.InvariantCulture, template, arguments));
            }
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => Array.Empty<LocalizedString>();
    }
}
