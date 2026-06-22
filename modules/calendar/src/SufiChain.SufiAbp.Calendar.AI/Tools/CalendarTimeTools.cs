using System.Globalization;
using SufiChain.SufiAbp.AI;
using SufiChain.SufiAbp.Calendar.Availability;
using SufiChain.SufiAbp.Calendar.Calendars;

namespace SufiChain.SufiAbp.Calendar.AI.Tools;

public class CalendarGetCurrentTimeTool : CalendarAIToolBase
{
    private static readonly PersianCalendar PersianCalendar = new();
    private readonly IAvailabilityCalendarAppService _availabilityCalendarAppService;

    public CalendarGetCurrentTimeTool(IAvailabilityCalendarAppService availabilityCalendarAppService)
    {
        _availabilityCalendarAppService = availabilityCalendarAppService;
    }

    public override string Name => CalendarAIToolNames.GetCurrentTime;

    public override string Description => CalendarAIToolGuidance.GetCurrentTime;

    public override string ParameterSchema => CalendarAIToolSchemas.GetCurrentTime;

    public override async Task<SufiAIToolExecutionResult> ExecuteAsync(
        SufiAIToolExecutionContext context,
        Dictionary<string, object?> parameters,
        CancellationToken cancellationToken = default)
    {
        var input = ReadInput<CalendarAIGetCurrentTimeInput>(parameters);
        return await SuccessAsync(await GetCurrentTimeAsync(input.CalendarId, input.TimeZoneId, cancellationToken));
    }

    [SufiAITool(CalendarAIToolNames.GetCurrentTime, "Returns the current UTC time plus current local date/time, weekday, timezone, and Persian/Jalali date for a calendar timezone or explicit timezone. Use this before interpreting relative-date words, next working-day requests, Farsi date phrases, Persian calendar dates, next Saturday, today, or tomorrow. Prefer calendarId so the timezone is inherited from the calendar. Do not calculate today's Jalali date from memory.")]
    public virtual async Task<object> GetCurrentTimeAsync(
        Guid? calendarId = null,
        string? timeZoneId = null,
        CancellationToken cancellationToken = default)
    {
        var resolvedTimeZoneId = await ResolveTimeZoneIdAsync(calendarId, timeZoneId, cancellationToken);
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(resolvedTimeZoneId);
        var utcNow = GetUtcNow();
        var localNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utcNow, DateTimeKind.Utc), timeZone);

        return new
        {
            UtcNow = utcNow,
            TimeZoneId = timeZone.Id,
            LocalNow = localNow,
            LocalDate = localNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            LocalTime = localNow.ToString("HH:mm:ss", CultureInfo.InvariantCulture),
            DayOfWeek = localNow.DayOfWeek.ToString(),
            PersianDate = FormatPersianDate(localNow),
            PersianDay = PersianCalendar.GetDayOfMonth(localNow),
            PersianMonth = PersianCalendar.GetMonth(localNow),
            PersianMonthName = GetPersianMonthName(PersianCalendar.GetMonth(localNow)),
            PersianYear = PersianCalendar.GetYear(localNow),
            PersianWeekdayName = GetPersianWeekdayName(localNow.DayOfWeek)
        };
    }

    protected virtual DateTime GetUtcNow()
    {
        return TimeProvider.System.GetUtcNow().UtcDateTime;
    }

    protected virtual async Task<string> ResolveTimeZoneIdAsync(
        Guid? calendarId,
        string? timeZoneId,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(timeZoneId))
        {
            return timeZoneId;
        }

        if (calendarId.HasValue)
        {
            var calendars = await _availabilityCalendarAppService.GetListAsync(new GetCalendarListInput
            {
                MaxResultCount = 50
            });

            var calendar = calendars.Items.FirstOrDefault(item => item.Id == calendarId.Value);
            if (calendar != null && !string.IsNullOrWhiteSpace(calendar.TimeZoneId))
            {
                return calendar.TimeZoneId;
            }
        }

        var defaultCalendar = await GetDefaultCalendarAsync(cancellationToken);
        return string.IsNullOrWhiteSpace(defaultCalendar?.TimeZoneId)
            ? TimeZoneInfo.Local.Id
            : defaultCalendar.TimeZoneId;
    }

    protected virtual async Task<CalendarDto?> GetDefaultCalendarAsync(CancellationToken cancellationToken)
    {
        var calendars = await _availabilityCalendarAppService.GetListAsync(new GetCalendarListInput
        {
            MaxResultCount = 10
        });

        return calendars.Items.FirstOrDefault(calendar => calendar.IsDefault)
               ?? calendars.Items.FirstOrDefault();
    }

    protected static string FormatPersianDate(DateTime localDateTime)
    {
        var year = PersianCalendar.GetYear(localDateTime);
        var month = PersianCalendar.GetMonth(localDateTime);
        var day = PersianCalendar.GetDayOfMonth(localDateTime);
        return $"{year:0000}-{month:00}-{day:00}";
    }

    protected static string GetPersianMonthName(int month)
    {
        return month switch
        {
            1 => "فروردین",
            2 => "اردیبهشت",
            3 => "خرداد",
            4 => "تیر",
            5 => "مرداد",
            6 => "شهریور",
            7 => "مهر",
            8 => "آبان",
            9 => "آذر",
            10 => "دی",
            11 => "بهمن",
            12 => "اسفند",
            _ => month.ToString(CultureInfo.InvariantCulture)
        };
    }

    protected static string GetPersianWeekdayName(DayOfWeek dayOfWeek)
    {
        return dayOfWeek switch
        {
            DayOfWeek.Saturday => "شنبه",
            DayOfWeek.Sunday => "یکشنبه",
            DayOfWeek.Monday => "دوشنبه",
            DayOfWeek.Tuesday => "سه‌شنبه",
            DayOfWeek.Wednesday => "چهارشنبه",
            DayOfWeek.Thursday => "پنجشنبه",
            DayOfWeek.Friday => "جمعه",
            _ => dayOfWeek.ToString()
        };
    }
}
